using Microsoft.AspNetCore.Identity;

namespace WeddingInvitation.Data;

public class ApplicationUser : IdentityUser
{
    public bool IsRemoved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
