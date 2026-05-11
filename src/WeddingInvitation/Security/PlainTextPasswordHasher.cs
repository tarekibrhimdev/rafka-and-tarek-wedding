using Microsoft.AspNetCore.Identity;

namespace WeddingInvitation.Security;

/// <summary>
/// Stores and verifies passwords as plain text. Intended only for this small admin-only app per operator request — not for production security best practices.
/// </summary>
public sealed class PlainTextPasswordHasher<TUser> : IPasswordHasher<TUser>
    where TUser : class
{
    public string HashPassword(TUser user, string password) => password;

    public PasswordVerificationResult VerifyHashedPassword(
        TUser user,
        string hashedPassword,
        string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            return PasswordVerificationResult.Failed;

        return string.Equals(hashedPassword, providedPassword, StringComparison.Ordinal)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }
}
