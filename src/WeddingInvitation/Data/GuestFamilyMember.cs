namespace WeddingInvitation.Data;

public class GuestFamilyMember : EntityBase
{
    public Guid GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
