namespace WeddingInvitation.Data;

public class Guest : EntityBase
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    public ICollection<GuestFamilyMember> FamilyMembers { get; set; } = new List<GuestFamilyMember>();
    public Invitation? Invitation { get; set; }
}
