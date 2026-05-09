namespace WeddingInvitation.Data;

public class Invitation : EntityBase
{
    public Guid GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public int MaxPersons { get; set; }

    public RsvpStatus RsvpStatus { get; set; } = RsvpStatus.Pending;
    public int? ComingCount { get; set; }
    public DateTime? RespondedAtUtc { get; set; }

    public ICollection<InvitationAuditEntry> AuditEntries { get; set; } = new List<InvitationAuditEntry>();
}
