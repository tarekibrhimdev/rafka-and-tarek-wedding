using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WeddingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
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

await DbInitializer.SeedAdminAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

/* Development: do not call UseHttpsRedirection — it pushes browsers to https://localhost where Edge often hits ERR_HTTP2_PROTOCOL_ERROR. Use http://localhost:7253 (WeddingInvitation launch profile). */

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    /* Physical wwwroot first — avoids MapStaticAssets chunked/HTTP2 edge cases for /cinematic-invite/ etc. */
    app.UseStaticFiles();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
