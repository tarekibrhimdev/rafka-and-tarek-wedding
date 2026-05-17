using WeddingInvitation.Data;

namespace WeddingInvitation.Services;

public static class SeatingHeadcount
{
    /// <summary>
    /// Seats consumed when this household sits together at one table.
    /// Approved + ComingCount → use that; declined → 0; otherwise estimate from household row + RSVP hint.
    /// </summary>
    public static int GetSeatCount(Guest guest, Invitation? invitation)
    {
        var householdOnRecord = 1 + guest.FamilyMembers.Count;

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
        var householdOnRecord = 1 + guest.FamilyMembers.Count;

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
