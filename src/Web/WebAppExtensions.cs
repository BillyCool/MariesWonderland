using MariesWonderland.Accounts;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.MasterEditing;
using MariesWonderland.Persistence;
using MariesWonderland.Web.Components;
using MariesWonderland.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using System.Text;

namespace MariesWonderland.Web;

/// <summary>
/// The web app on the same endpoint as the game API: the transfer site (<c>/bridge/…</c>, <c>/import</c>; static
/// server-rendered pages, no script) and the admin tool (<c>/admin</c>; interactive Blazor with MudBlazor, behind a
/// cookie login). See <c>docs/systems/web-app.md</c>.
/// </summary>
public static class WebAppExtensions
{
    public const string AdminScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    public static IServiceCollection AddWebApp(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WebOptions>(configuration.GetSection(WebOptions.SectionName));
        ServerOptions server = configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>() ?? new();

        // Keys for the login cookie and form tokens, kept next to the user database so a redeploy keeps people signed in.
        string keysFolder = Path.Combine(Path.GetDirectoryName(DataOptions.ResolvePath(server.Data.SqliteDatabase))!, "keys");
        services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysFolder)).SetApplicationName("MariesWonderland");

        services.AddAuthentication(AdminScheme).AddCookie(AdminScheme, options =>
        {
            options.Cookie.Name = "mw_admin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.LoginPath = "/admin/login";
            options.AccessDeniedPath = "/admin/login";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        services.AddAntiforgery(options => options.Cookie.Name = "mw_form");

        services.AddRazorComponents().AddInteractiveServerComponents();
        services.AddMudServices();

        services.AddSingleton<AccountStore>();
        services.AddSingleton<ActivityTracker>();
        services.AddSingleton<ActivityInterceptor>();
        services.AddSingleton<AccountStatusInterceptor>();
        services.AddHostedService<ActivityFlusher>();
        services.AddSingleton<SaveImportService>();
        services.AddSingleton<AttemptLimiter>();
        services.AddSingleton<AdminSetup>();
        services.AddSingleton<GameText>();
        services.AddSingleton<UserAdmin>();
        services.AddSingleton<ServerStats>();
        services.AddSingleton<MasterDraft>();
        services.AddSingleton<MasterDataPublisher>();
        return services;
    }

    /// <summary>The middleware the web app needs; call before the endpoints are mapped.</summary>
    public static WebApplication UseWebApp(this WebApplication app)
    {
        WebOptions web = app.Services.GetRequiredService<IOptions<WebOptions>>().Value;
        if (web.AdminPort > 0)
        {
            // The admin tool answers only on its own port (e.g. one published to the LAN only).
            app.Use(async (context, next) =>
            {
                if (IsAdminPath(context.Request.Path) && context.Connection.LocalPort != web.AdminPort)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                await next(context);
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        return app;
    }

    public static WebApplication MapWebApp(this WebApplication app)
    {
        app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        RouteGroupBuilder admin = app.MapGroup("/admin").RequireAuthorization();

        admin.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(AdminScheme);
            return Results.Redirect("/admin/login");
        });

        // Downloads the interactive pages link to.
        admin.MapGet("/download/user/{userId:long}", (long userId, UserAdmin users) =>
        {
            string? json = users.Export(userId);
            return json == null
                ? Results.NotFound()
                : Results.File(Encoding.UTF8.GetBytes(json), "application/json", $"user-{userId}.json");
        });

        admin.MapGet("/download/master/{table}", (string table, MasterDraft draft) =>
        {
            MasterTable? masterTable = MasterTables.Find(table);
            if (masterTable == null)
            {
                return Results.NotFound();
            }

            IReadOnlyList<object> rows = draft.Rows(masterTable);
            Array array = Array.CreateInstance(masterTable.RowType, rows.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                array.SetValue(rows[i], i);
            }
            string json = System.Text.Json.JsonSerializer.Serialize(array, array.GetType(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return Results.File(Encoding.UTF8.GetBytes(json), "application/json", $"{masterTable.Name}Table.json");
        });

        admin.MapGet("/download/master-file", (MasterDataPublisher publisher) =>
            File.Exists(publisher.FilePath)
                ? Results.File(publisher.FilePath, "application/octet-stream", Path.GetFileName(publisher.FilePath))
                : Results.NotFound());

        PrepareAdmins(app);
        app.Services.GetRequiredService<MasterDataPublisher>().ApplyStartupVersion();
        return app;
    }

    public static bool IsAdminPath(PathString path)
        => path.StartsWithSegments("/admin") || path.StartsWithSegments("/_blazor");

    /// <summary>
    /// The first administrator: from <c>Web:InitialAdmin</c> when set, otherwise a one-time setup link in the log.
    /// </summary>
    private static void PrepareAdmins(WebApplication app)
    {
        AccountStore accounts = app.Services.GetRequiredService<AccountStore>();
        if (accounts.AnyAdmin())
        {
            return;
        }

        WebOptions web = app.Services.GetRequiredService<IOptions<WebOptions>>().Value;
        if (!string.IsNullOrEmpty(web.InitialAdmin.Password))
        {
            accounts.SetAdmin(web.InitialAdmin.Name, web.InitialAdmin.Password);
            app.Logger.LogInformation("Created administrator {Name} from Web:InitialAdmin", web.InitialAdmin.Name);
            return;
        }

        string token = app.Services.GetRequiredService<AdminSetup>().Token;
        app.Logger.LogWarning("No administrator yet. Create one at /admin/setup?token={Token} (this link works until one exists)", token);
    }
}

/// <summary>The one-time token for creating the first administrator.</summary>
public sealed class AdminSetup
{
    public string Token { get; } = TransferCode.Random(24);

    public bool Check(string? token)
        => token != null && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(token), Encoding.ASCII.GetBytes(Token));
}

/// <summary>
/// Limits wrong transfer codes per client address: at most <see cref="MaxFailures"/> in <see cref="Window"/>. The
/// address is Cloudflare's <c>CF-Connecting-IP</c> when present (the firewall only lets Cloudflare in).
/// </summary>
public sealed class AttemptLimiter(TimeProvider timeProvider)
{
    public const int MaxFailures = 20;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Queue<DateTime>> _failures = new();

    public bool IsBlocked(string client)
    {
        if (!_failures.TryGetValue(client, out Queue<DateTime>? queue))
        {
            return false;
        }
        lock (queue)
        {
            Trim(queue);
            return queue.Count >= MaxFailures;
        }
    }

    public void Fail(string client)
    {
        Queue<DateTime> queue = _failures.GetOrAdd(client, _ => new Queue<DateTime>());
        lock (queue)
        {
            Trim(queue);
            queue.Enqueue(timeProvider.GetUtcNow().UtcDateTime);
        }
    }

    private void Trim(Queue<DateTime> queue)
    {
        DateTime cutoff = timeProvider.GetUtcNow().UtcDateTime - Window;
        while (queue.Count > 0 && queue.Peek() < cutoff)
        {
            queue.Dequeue();
        }
    }

    public static string ClientOf(HttpContext context)
        => context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
