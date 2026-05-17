using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Data;
using WeddingInvitation.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});

builder.Services.AddDbContext<WeddingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>, PlainTextPasswordHasher<ApplicationUser>>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<WeddingDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<WeddingInvitation.Services.InvitationLinkBuilder>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

var app = builder.Build();

app.UseForwardedHeaders();

await DbInitializer.SeedAdminAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

/* Admin UI: send users to login instead of a bare 403/401 page (must run before routing). */
app.UseStatusCodePages(async context =>
{
    var httpContext = context.HttpContext;
    if (httpContext.Response.HasStarted)
        return;

    var status = httpContext.Response.StatusCode;
    if (status != StatusCodes.Status403Forbidden && status != StatusCodes.Status401Unauthorized)
        return;

    var path = httpContext.Request.Path.Value ?? "";
    if (!path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
        return;

    if (path.StartsWith("/Account/", StringComparison.OrdinalIgnoreCase))
        return;

    var loginBase = $"{httpContext.Request.PathBase}/Account/Login";
    var returnUrl = $"{httpContext.Request.PathBase}{httpContext.Request.Path}{httpContext.Request.QueryString}";
    var target = QueryHelpers.AddQueryString(loginBase, "ReturnUrl", returnUrl);
    httpContext.Response.Redirect(target);
});

/* Physical wwwroot — serves /cinematic-invite/* reliably in dev and prod (MapStaticAssets alone can miss newly added files until rebuild). */
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
