namespace WeddingInvitation.Data;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsRemoved { get; set; }
}
