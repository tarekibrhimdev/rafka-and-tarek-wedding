namespace WeddingInvitation.Data;

public class ReceptionTable : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }

    public ICollection<GuestTableAssignment> Assignments { get; set; } = new List<GuestTableAssignment>();
}
