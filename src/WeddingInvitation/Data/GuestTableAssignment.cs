namespace WeddingInvitation.Data;

/// <summary>
/// Places one guest record (household / invitation party) at one reception table.
/// </summary>
public class GuestTableAssignment : EntityBase
{
    public Guid GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public Guid ReceptionTableId { get; set; }
    public ReceptionTable ReceptionTable { get; set; } = null!;
}
