using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Data;

namespace WeddingInvitation.Pages.Admin;

[Authorize]
public class IndexModel(WeddingDbContext db) : PageModel
{
    public int TotalGuests { get; private set; }
    public int InvitationsIssued { get; private set; }
    public int PendingRsvps { get; private set; }

    /// <summary>Sum of MaxPersons across all invitations — maximum headcount your links can cover (for seating capacity planning).</summary>
    public int SeatsInvited { get; private set; }

    /// <summary>Invitations with an accepted RSVP (one per household).</summary>
    public int ApprovedInvitations { get; private set; }

    /// <summary>Total people marked as attending across accepted RSVPs (sum of ComingCount).</summary>
    public int ConfirmedAttending { get; private set; }

    /// <summary>Invitations where the guest declined.</summary>
    public int DeclinedInvitations { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        TotalGuests = await db.Guests.AsNoTracking().CountAsync(cancellationToken);

        InvitationsIssued = await db.Invitations
            .AsNoTracking()
            .CountAsync(cancellationToken);

        PendingRsvps = await db.Invitations
            .AsNoTracking()
            .CountAsync(i => i.RsvpStatus == RsvpStatus.Pending, cancellationToken);

        SeatsInvited = await db.Invitations.AsNoTracking()
            .SumAsync(i => i.MaxPersons, cancellationToken);

        ApprovedInvitations = await db.Invitations.AsNoTracking()
            .CountAsync(i => i.RsvpStatus == RsvpStatus.Approved, cancellationToken);

        DeclinedInvitations = await db.Invitations.AsNoTracking()
            .CountAsync(i => i.RsvpStatus == RsvpStatus.Declined, cancellationToken);

        ConfirmedAttending = await db.Invitations.AsNoTracking()
            .Where(i => i.RsvpStatus == RsvpStatus.Approved)
            .SumAsync(i => i.ComingCount ?? 0, cancellationToken);
    }
}
