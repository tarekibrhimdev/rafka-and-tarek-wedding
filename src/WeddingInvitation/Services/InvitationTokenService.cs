using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace WeddingInvitation.Services;

public static class InvitationTokenService
{
    public static string CreateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }
}
