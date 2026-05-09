namespace WeddingInvitation.Services;

public class InvitationLinkBuilder(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string BuildInvitationUrl(string token)
    {
        var prefix = (_configuration["PublicSite:InvitationPathPrefix"] ?? "/invite").TrimEnd('/');
        var baseUrl = (_configuration["PublicSite:BaseUrl"] ?? string.Empty).TrimEnd('/');

        if (string.IsNullOrEmpty(baseUrl))
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.Request is null)
                return $"{prefix}/{token}";

            baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host.Value}";
        }

        return $"{baseUrl}{prefix}/{token}";
    }
}
