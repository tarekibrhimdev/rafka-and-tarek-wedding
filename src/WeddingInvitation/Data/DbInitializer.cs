using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WeddingInvitation.Data;

public static class DbInitializer
{
    public static async Task SeedAdminAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();

        var userName = configuration["Admin:UserName"] ?? "admin";
        var password = configuration["Admin:Password"] ?? "ChangeMe!123";

        var existing = await userManager.FindByNameAsync(userName);
        if (existing is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = null,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow,
            IsRemoved = false
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to seed admin user: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            throw new InvalidOperationException("Admin seed failed.");
        }

        logger.LogInformation("Seeded admin user {UserName}.", userName);
    }
}
