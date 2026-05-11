using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Data;

namespace WeddingInvitation.Pages;

public record PartySlotVm(int Index, string Label, string? I18nFormatArg);

[AllowAnonymous]
public class InviteModel(WeddingDbContext db, IConfiguration configuration, IWebHostEnvironment env) : PageModel
{
    private static readonly JsonSerializerOptions InviteCopyJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private const int CalendarYear = 2026;
    private const int CalendarMonth = 6;
    private const int HighlightCalendarDay = 27;

    public string InviteToken { get; private set; } = string.Empty;
    public string GuestDisplayName { get; private set; } = string.Empty;
    public int PartyMax { get; private set; } = 1;
    public IReadOnlyList<PartySlotVm> PartySlots { get; private set; } = [];
    /// <summary>True after a successful RSVP POST redirect (yes or no).</summary>
    public bool RsvpSuccess { get; private set; }

    /// <summary>True when the guest chose attending yes (excludes decline path).</summary>
    public bool RsvpAttendingYes { get; private set; }

    public string CoupleLine { get; private set; } = string.Empty;
    public string InviteCopyJson { get; private set; } = "{}";

    /// <summary>English strings for first paint before the client applies session language.</summary>
    public IReadOnlyDictionary<string, string> CopyEn { get; private set; } =
        new Dictionary<string, string>();

    /// <summary>Monday-first rows; null = empty cell.</summary>
    public IReadOnlyList<IReadOnlyList<int?>> CalendarRows { get; private set; } = [];

    public int CalendarHighlightDay => HighlightCalendarDay;

    /// <summary>Cache-busted URL for location-slide background (CSS url()).</summary>
    public string LocationHeroUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Lightweight static terrain preview (not an embed). Styled in CSS as monochrome editorial art — no Google UI, no iframe.
    /// </summary>
    public string ChurchStaticMapPreviewUrl { get; } =
        "https://staticmap.openstreetmap.de/staticmap.php?center=33.640,35.845&zoom=11&size=1280x800&maptype=mapnik";

    public string VenueStaticMapPreviewUrl { get; } =
        "https://staticmap.openstreetmap.de/staticmap.php?center=33.666088,35.737484&zoom=11&size=1280x800&maptype=mapnik";

    [BindProperty]
    public string RsvpName { get; set; } = "";

    [BindProperty]
    public string RsvpAttending { get; set; } = "";

    [BindProperty]
    public List<int> PartySlotIndexes { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return NotFound();

        var invitation = await db.Invitations
            .AsNoTracking()
            .Include(i => i.Guest)
            .ThenInclude(g => g.FamilyMembers)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

        if (invitation is null)
            return NotFound();

        var rsvpFlash = TempData["RsvpSuccess"] as string;
        // "yes" | "no" after submit; legacy "1" treated as yes-only success.
        RsvpSuccess = rsvpFlash is "yes" or "no" or "1";
        RsvpAttendingYes = rsvpFlash is "yes" or "1";
        await HydratePageModelAsync(token, invitation);
        return Page();
    }

    public async Task<IActionResult> OnPostRsvpAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return NotFound();

        var invitation = await db.Invitations
            .Include(i => i.Guest)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

        if (invitation is null)
            return NotFound();

        var max = Math.Max(1, invitation.MaxPersons);
        var name = (RsvpName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            ModelState.AddModelError(nameof(RsvpName), "Please enter your name.");

        var attending = (RsvpAttending ?? "").Trim().ToLowerInvariant();
        if (attending is not ("yes" or "no"))
            ModelState.AddModelError(nameof(RsvpAttending), "Please choose whether you are attending.");

        var indexes = (PartySlotIndexes ?? []).Distinct().Where(i => i >= 0 && i < max).ToList();
        if (ModelState.IsValid && attending == "yes")
        {
            if (max > 1 && indexes.Count == 0)
                ModelState.AddModelError(nameof(PartySlotIndexes), "Please select who will attend.");
            // Single-seat invite: no checkboxes; count as the guest of honour attending alone.
            if (max == 1)
                indexes = [0];
        }

        if (!ModelState.IsValid)
        {
            await HydratePageModelAfterPostErrorAsync(token, cancellationToken);
            GuestDisplayName = (RsvpName ?? "").Trim();
            return Page();
        }

        invitation.Guest.DisplayName = name;
        invitation.RsvpStatus = attending == "yes" ? RsvpStatus.Approved : RsvpStatus.Declined;
        invitation.ComingCount = attending == "yes" ? indexes.Count : 0;
        invitation.RespondedAtUtc = DateTime.UtcNow;

        var slotsDetail = attending == "yes" ? string.Join(",", indexes.Order()) : "";
        db.InvitationAuditEntries.Add(
            new InvitationAuditEntry
            {
                InvitationId = invitation.Id,
                EventType = InvitationAuditEventType.RsvpSubmitted,
                Details = $"attending={attending};count={invitation.ComingCount};slots=[{slotsDetail}]",
            });

        await db.SaveChangesAsync(cancellationToken);
        TempData["RsvpSuccess"] = attending == "yes" ? "yes" : "no";
        return RedirectToPage(new { token });
    }

    private async Task HydratePageModelAfterPostErrorAsync(string token, CancellationToken cancellationToken)
    {
        var invitation = await db.Invitations
            .AsNoTracking()
            .Include(i => i.Guest)
            .ThenInclude(g => g.FamilyMembers)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
        if (invitation is null)
            return;
        await HydratePageModelAsync(token, invitation);
    }

    private Task HydratePageModelAsync(string token, Invitation invitation)
    {
        InviteToken = token;
        GuestDisplayName = invitation.Guest.DisplayName;
        CoupleLine = configuration["Wedding:CoupleLine"] ?? "Tarek & Rafka";
        CalendarRows = BuildCalendarGrid(CalendarYear, CalendarMonth);
        var coupleLineAr = configuration["Wedding:CoupleLineAr"];
        var allCopy = InviteCopyDefaults.Build(CoupleLine, coupleLineAr);
        CopyEn = allCopy["en"];
        InviteCopyJson = JsonSerializer.Serialize(allCopy, InviteCopyJsonOptions);
        PartySlots = BuildPartySlots(invitation, CopyEn);
        PartyMax = Math.Max(1, invitation.MaxPersons);

        var heroFs = Path.Combine(env.WebRootPath, "cinematic-invite", "images", "location-couple-hero.png");
        var heroVer = global::System.IO.File.Exists(heroFs) ? global::System.IO.File.GetLastWriteTimeUtc(heroFs).Ticks : 0L;
        LocationHeroUrl = $"{Url.Content("~/cinematic-invite/images/location-couple-hero.png")}?v={heroVer}";

        return Task.CompletedTask;
    }

    private static IReadOnlyList<PartySlotVm> BuildPartySlots(
        Invitation invitation,
        IReadOnlyDictionary<string, string> enCopy)
    {
        var max = Math.Max(1, invitation.MaxPersons);
        var extraFmt =
            enCopy.TryGetValue("cin_rsvp_party_extra_fmt", out var ef) ? ef : "Guest {0}";

        // One checkbox per GuestFamilyMembers row (admin). "Invitation for" uses Guest.DisplayName in the name field only.
        var slots = new List<PartySlotVm>();
        var members = invitation.Guest.FamilyMembers.OrderBy(m => m.SortOrder).ToList();
        foreach (var m in members)
        {
            if (slots.Count >= max)
                break;
            var label = (m.FullName ?? "").Trim();
            if (label.Length == 0)
                continue;
            slots.Add(new PartySlotVm(slots.Count, label, null));
        }

        var n = 1;
        while (slots.Count < max)
        {
            var idx = slots.Count;
            slots.Add(new PartySlotVm(idx, string.Format(extraFmt, n), n.ToString()));
            n++;
        }

        return slots;
    }

    private static List<List<int?>> BuildCalendarGrid(int year, int month)
    {
        var first = new DateTime(year, month, 1);
        var leading = ((int)first.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var dim = DateTime.DaysInMonth(year, month);
        var cells = new List<int?>();
        for (var i = 0; i < leading; i++)
            cells.Add(null);
        for (var d = 1; d <= dim; d++)
            cells.Add(d);
        while (cells.Count % 7 != 0)
            cells.Add(null);

        var rows = new List<List<int?>>();
        for (var i = 0; i < cells.Count; i += 7)
            rows.Add(cells.Skip(i).Take(7).ToList());

        return rows;
    }
}
