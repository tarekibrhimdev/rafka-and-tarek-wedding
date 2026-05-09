namespace WeddingInvitation.Data;

public class InvitationAuditEntry : EntityBase
{
    public Guid InvitationId { get; set; }
    public Invitation Invitation { get; set; } = null!;

    public InvitationAuditEventType EventType { get; set; }
    public string? Details { get; set; }
}
