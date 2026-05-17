using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Data;
using WeddingInvitation.Services;

namespace WeddingInvitation.Pages.Admin.Seating;

[Authorize]
public class IndexModel(WeddingDbContext db) : PageModel
{
    public IReadOnlyList<TableVm> Tables { get; private set; } = [];
    public IReadOnlyList<UnassignedVm> UnassignedGuests { get; private set; } = [];

    public int TotalTableCapacity { get; private set; }
    public int TotalSeatsAssigned { get; private set; }
    public int UnassignedHouseholdCount => UnassignedGuests.Count;

    public CreateTableInput CreateTable { get; set; } = new();
    public EditTableInput EditTable { get; set; } = new();

    [BindProperty]
    public Guid AssignGuestId { get; set; }

    [BindProperty]
    public Guid AssignTableId { get; set; }

    [BindProperty]
    public Guid UnassignAssignmentId { get; set; }

    [BindProperty]
    public Guid RemoveTableId { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        await LoadPageAsync(cancellationToken);

    public async Task<IActionResult> OnPostCreateTableAsync(
        [Bind(Prefix = nameof(CreateTable))] CreateTableInput input,
        CancellationToken cancellationToken)
    {
        CreateTable = input;
        if (!TryValidateModel(input, nameof(CreateTable)))
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        var maxOrder = await db.ReceptionTables.Select(t => (int?)t.SortOrder).MaxAsync(cancellationToken) ?? 0;

        db.ReceptionTables.Add(new ReceptionTable
        {
            Name = input.Name.Trim(),
            Capacity = input.Capacity,
            SortOrder = maxOrder + 1
        });

        await db.SaveChangesAsync(cancellationToken);
        TempData["Toast"] = $"Table “{input.Name.Trim()}” added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateTableAsync(
        [Bind(Prefix = nameof(EditTable))] EditTableInput input,
        CancellationToken cancellationToken)
    {
        EditTable = input;

        if (!TryValidateModel(input, nameof(EditTable)))
        {
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        var table = await db.ReceptionTables.FirstOrDefaultAsync(t => t.Id == input.TableId, cancellationToken);
        if (table is null)
            return NotFound();

        var used = await SumUsedSeatsOnTableAsync(input.TableId, cancellationToken);
        if (input.Capacity < used)
        {
            ModelState.AddModelError($"{nameof(EditTable)}.{nameof(EditTableInput.Capacity)}",
                $"Capacity must be at least {used} (current seated headcount).");
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        table.Name = input.Name.Trim();
        table.Capacity = input.Capacity;
        await db.SaveChangesAsync(cancellationToken);

        TempData["Toast"] = $"Table “{input.Name.Trim()}” updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveTableAsync(CancellationToken cancellationToken)
    {
        if (RemoveTableId == Guid.Empty)
            return BadRequest();

        var table = await db.ReceptionTables.FirstOrDefaultAsync(t => t.Id == RemoveTableId, cancellationToken);
        if (table is null)
            return NotFound();

        table.IsRemoved = true;

        var assigns = await db.GuestTableAssignments
            .Where(a => a.ReceptionTableId == RemoveTableId && !a.IsRemoved)
            .ToListAsync(cancellationToken);

        foreach (var a in assigns)
            a.IsRemoved = true;

        await db.SaveChangesAsync(cancellationToken);
        TempData["Toast"] = $"Table “{table.Name}” removed; households are unassigned.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignAsync(CancellationToken cancellationToken)
    {
        if (AssignGuestId == Guid.Empty || AssignTableId == Guid.Empty)
            return BadRequest();

        var guest = await db.Guests
            .Include(g => g.FamilyMembers)
            .Include(g => g.Invitation)
            .FirstOrDefaultAsync(g => g.Id == AssignGuestId, cancellationToken);

        var table = await db.ReceptionTables.FirstOrDefaultAsync(t => t.Id == AssignTableId, cancellationToken);

        if (guest is null || table is null)
            return NotFound();

        var seats = SeatingHeadcount.GetSeatCount(guest, guest.Invitation);
        if (seats <= 0)
        {
            ModelState.AddModelError(string.Empty, "This household has 0 seats (declined or invalid); assign another row.");
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        var already = await db.GuestTableAssignments.AnyAsync(
            a => a.GuestId == AssignGuestId && !a.IsRemoved,
            cancellationToken);

        if (already)
        {
            ModelState.AddModelError(string.Empty, "That household is already seated.");
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        var used = await SumUsedSeatsOnTableAsync(AssignTableId, cancellationToken);
        if (used + seats > table.Capacity)
        {
            ModelState.AddModelError(string.Empty,
                $"Not enough seats on “{table.Name}” ({table.Capacity - used} left; need {seats}).");
            await LoadPageAsync(cancellationToken);
            return Page();
        }

        db.GuestTableAssignments.Add(new GuestTableAssignment
        {
            GuestId = guest.Id,
            ReceptionTableId = table.Id
        });

        await db.SaveChangesAsync(cancellationToken);
        TempData["Toast"] = $"Assigned “{guest.DisplayName}” to “{table.Name}”.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnassignAsync(CancellationToken cancellationToken)
    {
        if (UnassignAssignmentId == Guid.Empty)
            return BadRequest();

        var assignment = await db.GuestTableAssignments
            .Include(a => a.Guest)
            .Include(a => a.ReceptionTable)
            .FirstOrDefaultAsync(a => a.Id == UnassignAssignmentId, cancellationToken);

        if (assignment is null)
            return NotFound();

        assignment.IsRemoved = true;
        await db.SaveChangesAsync(cancellationToken);

        TempData["Toast"] =
            $"Removed “{assignment.Guest.DisplayName}” from “{assignment.ReceptionTable.Name}”.";
        return RedirectToPage();
    }

    private async Task<int> SumUsedSeatsOnTableAsync(Guid tableId, CancellationToken cancellationToken)
    {
        var rows = await db.GuestTableAssignments
            .Include(a => a.Guest).ThenInclude(g => g.FamilyMembers)
            .Include(a => a.Guest).ThenInclude(g => g.Invitation)
            .Where(a => !a.IsRemoved && a.ReceptionTableId == tableId)
            .ToListAsync(cancellationToken);

        return rows.Sum(a => SeatingHeadcount.GetSeatCount(a.Guest, a.Guest.Invitation));
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        var tables = await db.ReceptionTables
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var tableIds = tables.Select(t => t.Id).ToHashSet();

        var assignments = await db.GuestTableAssignments
            .Include(a => a.Guest).ThenInclude(g => g.FamilyMembers)
            .Include(a => a.Guest).ThenInclude(g => g.Invitation)
            .Where(a => !a.IsRemoved && tableIds.Contains(a.ReceptionTableId))
            .ToListAsync(cancellationToken);

        var byTable = assignments
            .GroupBy(a => a.ReceptionTableId)
            .ToDictionary(g => g.Key, g => g.ToList());

        TotalTableCapacity = tables.Sum(t => t.Capacity);
        TotalSeatsAssigned = assignments.Sum(a =>
            SeatingHeadcount.GetSeatCount(a.Guest, a.Guest.Invitation));

        var assignedGuestIds = assignments.Select(a => a.GuestId).ToHashSet();

        var unassigned = await db.Guests
            .Include(g => g.FamilyMembers)
            .Include(g => g.Invitation)
            .Where(g => !assignedGuestIds.Contains(g.Id))
            .OrderBy(g => g.DisplayName)
            .ToListAsync(cancellationToken);

        Tables = tables.Select(t =>
        {
            var rows = byTable.GetValueOrDefault(t.Id) ?? [];
            var used = rows.Sum(a => SeatingHeadcount.GetSeatCount(a.Guest, a.Guest.Invitation));
            var lineVms = rows
                .OrderBy(a => a.Guest.DisplayName)
                .Select(a => new AssignedGuestLineVm(
                    a.Id,
                    a.GuestId,
                    a.Guest.DisplayName,
                    SeatingHeadcount.GetSeatCount(a.Guest, a.Guest.Invitation),
                    SeatingHeadcount.GetSeatBasisNote(a.Guest, a.Guest.Invitation)))
                .ToList();

            return new TableVm(t.Id, t.Name, t.Capacity, t.SortOrder, used, Math.Max(0, t.Capacity - used), lineVms);
        }).ToList();

        UnassignedGuests = unassigned.Select(g =>
        {
            var seats = SeatingHeadcount.GetSeatCount(g, g.Invitation);
            var basis = SeatingHeadcount.GetSeatBasisNote(g, g.Invitation);
            return new UnassignedVm(
                g.Id,
                g.DisplayName,
                seats,
                basis,
                seats > 0,
                RsvpLabel(g.Invitation));
        }).ToList();
    }

    private static string RsvpLabel(Invitation? inv)
    {
        if (inv is null)
            return "No invite";
        return inv.RsvpStatus switch
        {
            RsvpStatus.Approved => "Approved",
            RsvpStatus.Declined => "Declined",
            _ => "Pending"
        };
    }
}

public sealed record TableVm(
    Guid Id,
    string Name,
    int Capacity,
    int SortOrder,
    int UsedSeats,
    int RemainingSeats,
    IReadOnlyList<AssignedGuestLineVm> AssignedGuests);

public sealed record AssignedGuestLineVm(
    Guid AssignmentId,
    Guid GuestId,
    string DisplayName,
    int Seats,
    string BasisNote);

public sealed record UnassignedVm(
    Guid GuestId,
    string DisplayName,
    int Seats,
    string BasisNote,
    bool CanAssign,
    string RsvpLabel);

public sealed class CreateTableInput
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500.")]
    public int Capacity { get; set; } = 8;
}

public sealed class EditTableInput
{
    [Required]
    public Guid TableId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500.")]
    public int Capacity { get; set; }
}
