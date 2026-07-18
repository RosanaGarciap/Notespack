using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NOTESPACK.Data;
using NOTESPACK.Models;
using NOTESPACK.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<EventContext>(options => options.UseSqlite("Data Source=notespack.db"));
builder.Services.AddHttpClient();

// The live sync against calendar.byu.edu only runs on your local machine (Development).
// In production (Azure) that API blocks requests by IP (403), so campus events
// are loaded there from a seed file instead (see CampusEventSeeder below).
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<NOTESPACK.Services.CampusEventSyncService>();
}

// Cookie configuration (native authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { 
        options.Cookie.HttpOnly = true; 
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(2); // change to 15 later
        options.SlidingExpiration = true; // each request renews the countdown 
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState(); // Required for <AuthorizeView>
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>(provider => 
    (CustomAuthenticationStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());

var app = builder.Build();

// DB initialization
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<EventContext>>();
    contextFactory.CreateDbContext().Database.Migrate();

    // Load campus events from the seed file. This works the same way locally
    // and in Azure, and doesn't depend on calling any external API at startup.
    var seedPath = Path.Combine(app.Environment.ContentRootPath, "Data", "campus-events-seed.json");
    await CampusEventSeeder.SeedFromFileAsync(contextFactory, seedPath);
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication(); // Enables authentication
app.UseAuthorization();  // Enables authorization

// Development-only endpoint: exports the current campus events (UserId == null)
// to Data/campus-events-seed.json. Run the app locally, hit this URL in the
// browser, then commit + push the updated file.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/admin/export-campus-events-seed", async (IDbContextFactory<EventContext> contextFactory) =>
    {
        using var context = contextFactory.CreateDbContext();

        var events = await context.Events
            .Where(e => e.UserId == null)
            .OrderBy(e => e.Date)
            .Select(e => new CampusEventSeedDto(e.Title, e.Description, e.Date, e.EndDate, e.Duration, e.Location, e.Organizer))
            .ToListAsync();

        var dataDir = Path.Combine(app.Environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        var path = Path.Combine(dataDir, "campus-events-seed.json");

        var json = JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);

        return Results.Ok($"Exported {events.Count} campus events to {path}");
    });
}

// Session-handling endpoints (backend)
app.MapPost("/Account/Login", async (HttpContext context, AuthService authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString() ?? "";
    var password = form["password"].ToString() ?? "";

    var result = await authService.LoginAsync(email, password);

    if (result.Status == AuthLoginStatus.Success)
    {
        var user = result.User!;
        var userId = user.Id.ToString();

        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", userId)
        };

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        context.Response.Redirect("/");
    }
    else if (result.Status == AuthLoginStatus.EmailNotFound)
    {
        context.Response.Redirect("/login?error=email_not_found");
    }
    else // WrongPassword
    {
        context.Response.Redirect("/login?error=wrong_password");
    }
});

app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.MapPost("/Account/InactivityLogout", async (HttpContext context) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
    return Results.Ok();
});

app.MapRazorComponents<NOTESPACK.App>().AddInteractiveServerRenderMode();
app.Run();