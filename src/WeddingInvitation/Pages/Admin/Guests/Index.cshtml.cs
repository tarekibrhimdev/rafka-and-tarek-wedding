using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Data;
using WeddingInvitation.Services;

namespace WeddingInvitation.Pages.Admin.Guests;

[Authorize]
public class IndexModel(
    WeddingDbContext db,
    InvitationLinkBuilder linkBuilder) : PageModel
{
    public IReadOnlyList<GuestRowVm> Guests { get; private set; } = [];

    /// <summary>Total guests in the database (ignores list filters).</summary>
    public int TotalGuestsAll { get; private set; }

    /// <summary>Σ MaxPersons for rows with an invitation (matches current list filters).</summary>
    public int ListCapacityOffered { get; private set; }

    /// <summary>Σ ComingCount for approved RSVPs in the current list (missing counts treated as 0).</summary>
    public int ListConfirmedAttending { get; private set; }

    /// <summary>Rows in the current list that have a generated invitation.</summary>
    public int ListRowsWithInvitation { get; private set; }

    /// <summary>Approved invitations in the list where ComingCount is still null.</summary>
    public int ListApprovedMissingHeadcount { get; private set; }

    [BindProperty(Name = "q", SupportsGet = true)]
    public string? GuestSearch { get; set; }

    /// <summary>all | no-invite | pending | approved | declined</summary>
    [BindProperty(Name = "status", SupportsGet = true)]
    public string StatusFilter { get; set; } = "all";

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(GuestSearch)
        || !string.Equals(NormalizeStatusFilter(StatusFilter), "all", StringComparison.OrdinalIgnoreCase);

    public bool IsStatusFilter(string expected) =>
        string.Equals(NormalizeStatusFilter(StatusFilter), expected, StringComparison.OrdinalIgnoreCase);

    /// <summary>Value to round-trip in POST forms (normalized status slug).</summary>
    public string StatusFilterNormalized => NormalizeStatusFilter(StatusFilter);

    /// <summary>
    /// Used for GET / invalid POST repopulation (not bound globally — avoids cross-handler validation failures).
    /// </summary>
    public GuestCreateInput CreateInput { get; set; } = new();

    public GuestEditInput EditInput { get; set; } = new();

    [BindProperty]
    public Guid DeleteGuestId { get; set; }

    [BindProperty]
    public Guid GenerateGuestId { get; set; }

    [BindProperty]
    public int MaxPersons { get; set; } = 2;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await PreparePageListAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetGuestJsonAsync(Guid id, CancellationToken cancellationToken)
    {
        var guest = await db.Guests
            .AsNoTracking()
            .Include(g => g.FamilyMembers)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (guest is null)
            return NotFound();

        var dto = new GuestDetailDto(
            guest.Id,
            guest.DisplayName,
            guest.Email,
            guest.Phone,
            guest.Notes,
            guest.FamilyMembers
                .OrderBy(m => m.SortOrder)
                .Select(m => new FamilyMemberLineDto(m.Id, m.FullName, m.SortOrder))
                .ToList());

        return new JsonResult(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public async Task<IActionResult> OnPostCreateAsync(
        [Bind(Prefix = nameof(CreateInput))] GuestCreateInput createInput,
        CancellationToken cancellationToken)
    {
        CreateInput = createInput;

        if (!TryValidateModel(createInput, nameof(CreateInput)))
        {
            await PreparePageListAsync(cancellationToken);
            return Page();
        }

        var names = NormalizeFamilyNames(createInput.FamilyMemberNames);

        var guest = new Guest
        {
            DisplayName = createInput.DisplayName.Trim(),
            Email = string.IsNullOrWhiteSpace(createInput.Email) ? null : createInput.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(createInput.Phone) ? null : createInput.Phone.Trim(),
            Notes = string.IsNullOrWhiteSpace(createInput.Notes) ? null : createInput.Notes.Trim()
        };

        var order = 0;
        foreach (var name in names)
        {
            guest.FamilyMembers.Add(new GuestFamilyMember
            {
                FullName = name,
                SortOrder = order++
            });
        }

        db.Guests.Add(guest);
        await db.SaveChangesAsync(cancellationToken);

        TempData["Toast"] = $"Guest “{guest.DisplayName}” created.";
        return RedirectToPage(FilterRoute());
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        [Bind(Prefix = nameof(EditInput))] GuestEditInput editInput,
        CancellationToken cancellationToken)
    {
        EditInput = editInput;

        if (!TryValidateModel(editInput, nameof(EditInput)))
        {
            await PreparePageListAsync(cancellationToken);
            return Page();
        }

        var guest = await db.Guests
            .FirstOrDefaultAsync(g => g.Id == editInput.GuestId, cancellationToken);

        if (guest is null)
            return NotFound();

        guest.DisplayName = editInput.DisplayName.Trim();
        guest.Email = string.IsNullOrWhiteSpace(editInput.Email) ? null : editInput.Email.Trim();
        guest.Phone = string.IsNullOrWhiteSpace(editInput.Phone) ? null : editInput.Phone.Trim();
        guest.Notes = string.IsNullOrWhiteSpace(editInput.Notes) ? null : editInput.Notes.Trim();

        var existingMembers = await db.GuestFamilyMembers
            .IgnoreQueryFilters()
            .Where(m => m.GuestId == guest.Id && !m.IsRemoved)
            .ToListAsync(cancellationToken);

        foreach (var m in existingMembers)
            m.IsRemoved = true;

        var names = NormalizeFamilyNames(editInput.FamilyMemberNames);
        var order = 0;
        foreach (var name in names)
        {
            db.GuestFamilyMembers.Add(new GuestFamilyMember
            {
                GuestId = guest.Id,
                FullName = name,
                SortOrder = order++
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        TempData["Toast"] = $"Guest “{guest.DisplayName}” updated.";
        return RedirectToPage(FilterRoute());
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken)
    {
        if (DeleteGuestId == Guid.Empty)
            return BadRequest();

        var guest = await db.Guests
            .Include(g => g.FamilyMembers)
            .Include(g => g.Invitation)
            .FirstOrDefaultAsync(g => g.Id == DeleteGuestId, cancellationToken);

        if (guest is null)
            return NotFound();

        guest.IsRemoved = true;

        foreach (var member in guest.FamilyMembers)
            member.IsRemoved = true;

        if (guest.Invitation is not null)
        {
            guest.Invitation.AuditEntries.Add(new InvitationAuditEntry
            {
                EventType = InvitationAuditEventType.InvitationRevoked,
                Details = "Guest removed"
            });
            guest.Invitation.IsRemoved = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        TempData["Toast"] = $"Guest “{guest.DisplayName}” removed.";
        return RedirectToPage(FilterRoute());
    }

    public async Task<IActionResult> OnPostGenerateInvitationAsync(CancellationToken cancellationToken)
    {
        if (MaxPersons < 1 || MaxPersons > 99)
        {
            ModelState.AddModelError(string.Empty, "Party size must be between 1 and 99.");
            await PreparePageListAsync(cancellationToken);
            return Page();
        }

        var guest = await db.Guests.FirstOrDefaultAsync(g => g.Id == GenerateGuestId, cancellationToken);
        if (guest is null)
            return NotFound();

        var existingInv = await db.Invitations.FirstOrDefaultAsync(i => i.GuestId == GenerateGuestId, cancellationToken);
        if (existingInv is not null)
        {
            existingInv.AuditEntries.Add(new InvitationAuditEntry
            {
                EventType = InvitationAuditEventType.InvitationRevoked,
                Details = "Regenerated from admin"
            });
            existingInv.IsRemoved = true;
        }

        var invitation = new Invitation
        {
            GuestId = guest.Id,
            Token = InvitationTokenService.CreateToken(),
            MaxPersons = MaxPersons,
            RsvpStatus = RsvpStatus.Pending,
            ComingCount = null,
            RespondedAtUtc = null
        };

        invitation.AuditEntries.Add(new InvitationAuditEntry
        {
            EventType = InvitationAuditEventType.InvitationGenerated,
            Details = $"MaxPersons={MaxPersons}"
        });

        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(cancellationToken);

        TempData["Toast"] = $"Invitation generated for “{guest.DisplayName}”.";
        TempData["HighlightGuestId"] = guest.Id.ToString();
        return RedirectToPage(FilterRoute());
    }

    private async Task PreparePageListAsync(CancellationToken cancellationToken)
    {
        TotalGuestsAll = await db.Guests.AsNoTracking().CountAsync(cancellationToken);
        Guests = await LoadGuestRowsAsync(cancellationToken);
        ComputeListCapacityStats();
    }

    private void ComputeListCapacityStats()
    {
        var withInvite = Guests.Where(g => g.HasInvitation).ToList();
        ListRowsWithInvitation = withInvite.Count;
        ListCapacityOffered = withInvite.Sum(g => g.MaxPersons);
        ListConfirmedAttending = withInvite
            .Where(g => g.RsvpStatus == RsvpStatus.Approved)
            .Sum(g => g.ComingCount ?? 0);
        ListApprovedMissingHeadcount = withInvite.Count(g =>
            g.RsvpStatus == RsvpStatus.Approved && g.ComingCount is null);
    }

    private object? FilterRoute()
    {
        var q = string.IsNullOrWhiteSpace(GuestSearch) ? null : GuestSearch.Trim();
        var st = NormalizeStatusFilter(StatusFilter);
        if (q is null && string.Equals(st, "all", StringComparison.OrdinalIgnoreCase))
            return null;
        return new { q, status = st };
    }

    private static string NormalizeStatusFilter(string? value)
    {
        var s = (value ?? "all").Trim().ToLowerInvariant();
        return s switch
        {
            "no-invite" or "pending" or "approved" or "declined" or "all" => s,
            _ => "all"
        };
    }

    private async Task<IReadOnlyList<GuestRowVm>> LoadGuestRowsAsync(CancellationToken cancellationToken)
    {
        var term = GuestSearch?.Trim();
        var status = NormalizeStatusFilter(StatusFilter);

        IQueryable<Guest> query = db.Guests.AsNoTracking();

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(g =>
                g.DisplayName.Contains(term)
                || (g.Email != null && g.Email.Contains(term))
                || (g.Phone != null && g.Phone.Contains(term))
                || g.FamilyMembers.Any(m => m.FullName.Contains(term)));
        }

        switch (status)
        {
            case "no-invite":
                query = query.Where(g => g.Invitation == null);
                break;
            case "pending":
                query = query.Where(g => g.Invitation != null && g.Invitation.RsvpStatus == RsvpStatus.Pending);
                break;
            case "approved":
                query = query.Where(g => g.Invitation != null && g.Invitation.RsvpStatus == RsvpStatus.Approved);
                break;
            case "declined":
                query = query.Where(g => g.Invitation != null && g.Invitation.RsvpStatus == RsvpStatus.Declined);
                break;
        }

        var guests = await query
            .Include(g => g.FamilyMembers)
            .Include(g => g.Invitation)
            .OrderBy(g => g.DisplayName)
            .ToListAsync(cancellationToken);

        return guests.Select(g =>
        {
            var inv = g.Invitation;
            var url = inv is null ? null : linkBuilder.BuildInvitationUrl(inv.Token);
            var familyCount = g.FamilyMembers.Count;
            var totalPersonCount = SeatingHeadcount.GetHouseholdPersonEstimate(g);
            return new GuestRowVm(
                g.Id,
                g.DisplayName,
                g.Email,
                g.Phone,
                familyCount,
                totalPersonCount,
                inv is not null,
                url,
                inv?.Token,
                inv?.MaxPersons ?? 0,
                inv?.RsvpStatus ?? RsvpStatus.Pending,
                inv?.ComingCount);
        }).ToList();
    }

    private static List<string> NormalizeFamilyNames(IEnumerable<string>? lines)
    {
        return lines?
                   .Select(s => s != null ? s.Trim() : "")
                   .Where(s => s.Length > 0)
                   .ToList()
               ?? [];
    }
}

public sealed record GuestRowVm(
    Guid Id,
    string DisplayName,
    string? Email,
    string? Phone,
    int FamilyMemberCount,
    int TotalPersonCount,
    bool HasInvitation,
    string? InvitationUrl,
    string? Token,
    int MaxPersons,
    RsvpStatus RsvpStatus,
    int? ComingCount);

public sealed class GuestCreateInput
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public List<string> FamilyMemberNames { get; set; } = [];
}

public sealed class GuestEditInput
{
    [Required]
    public Guid GuestId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public List<string> FamilyMemberNames { get; set; } = [];
}

public sealed record GuestDetailDto(
    Guid Id,
    string DisplayName,
    string? Email,
    string? Phone,
    string? Notes,
    IReadOnlyList<FamilyMemberLineDto> FamilyMembers);

public sealed record FamilyMemberLineDto(Guid Id, string FullName, int SortOrder);
