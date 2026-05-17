using WeddingInvitation.Data;

namespace WeddingInvitation.Services;

public static class SeatingHeadcount
{
    /// <summary>
    /// Persons counted from the guest record when RSVP does not supply a headcount.
    /// Matches the admin Guests list: each named household row counts as one person; no rows ⇒ primary guest only (1).
    /// </summary>
    public static int GetHouseholdPersonEstimate(Guest guest)
    {
        var n = guest.FamilyMembers.Count;
        return n == 0 ? 1 : n;
    }

    /// <summary>
    /// Seats consumed when this household sits together at one table.
    /// Approved + ComingCount → use that; declined → 0; otherwise estimate from guest record (see <see cref="GetHouseholdPersonEstimate"/>).
    /// </summary>
    public static int GetSeatCount(Guest guest, Invitation? invitation)
    {
        var householdOnRecord = GetHouseholdPersonEstimate(guest);

        if (invitation is null)
            return householdOnRecord;

        return invitation.RsvpStatus switch
        {
            RsvpStatus.Declined => 0,
            RsvpStatus.Approved => invitation.ComingCount ?? householdOnRecord,
            _ => householdOnRecord
        };
    }

    public static string GetSeatBasisNote(Guest guest, Invitation? invitation)
    {
        var householdOnRecord = GetHouseholdPersonEstimate(guest);

        if (invitation is null)
            return "Household on record (no invite).";

        return invitation.RsvpStatus switch
        {
            RsvpStatus.Declined => "Declined — not seated.",
            RsvpStatus.Approved when invitation.ComingCount is int n =>
                $"RSVP: {n} attending.",
            RsvpStatus.Approved =>
                $"Approved — headcount not set; using household ({householdOnRecord}).",
            _ => $"RSVP pending — estimate {householdOnRecord} from household."
        };
    }
}
