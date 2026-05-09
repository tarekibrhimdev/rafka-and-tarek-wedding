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

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        TotalGuests = await db.Guests.AsNoTracking().CountAsync(cancellationToken);

        InvitationsIssued = await db.Invitations
            .AsNoTracking()
            .CountAsync(cancellationToken);

        PendingRsvps = await db.Invitations
            .AsNoTracking()
            .CountAsync(i => i.RsvpStatus == RsvpStatus.Pending, cancellationToken);
    }
}
