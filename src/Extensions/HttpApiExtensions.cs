using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MariesWonderland.Extensions;

/// <summary>
/// Registers all non-gRPC HTTP endpoints: asset serving (list.bin, asset bundles, master database),
/// terms of service, and the catch-all fallback route. Debug save point endpoints are mapped separately
/// by <see cref="MapDebugApis"/> when dev mode allows it.
/// </summary>
public static class HttpApiExtensions
{
    /// <summary>
    /// Registers all HTTP endpoints, including asset serving, master database, and the catch-all route. The asset
    /// catalogue starts loading in the background, so the client's first asset request doesn't wait for it.
    /// </summary>
    public static WebApplication MapHttpApis(this WebApplication app)
    {
        ServerOptions options = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
        AssetDatabase assetDb = new(options.Paths.AssetDatabase, app.Services.GetRequiredService<ILogger<AssetDatabase>>());
        _ = Task.Run(assetDb.Prewarm);

        // ToS. Expects the version wrapped in delimiters like "###123###".
        app.MapGet("/web/static/{languagePath}/terms/termsofuse", (string languagePath) => $"<html><head><title>Terms of Service</title></head><body><h1>Terms of Service</h1><p>Language: {languagePath}</p><p>Version: ###123###</p></body></html>");

        app.MapAssetApis(options.Paths, assetDb);

        // Catch all
        app.MapMethods("/{**catchAll}", ["GET", "POST", "PUT", "DELETE", "PATCH",], (HttpContext ctx, string catchAll) =>
        {
            app.Logger.LogWarning("Catchall endpoint hit for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            return $"You requested: {catchAll}";
        });

        return app;
    }

    /// <summary>
    /// Registers the asset endpoints: list.bin for each revision, asset bundles and resources (looked up in the
    /// catalogue, see <see cref="AssetDatabase"/>), and the master database.
    /// </summary>
    public static WebApplication MapAssetApis(this WebApplication app, PathsOptions paths, AssetDatabase assetDb)
    {
        string masterDatabaseBasePath = string.IsNullOrEmpty(paths.MasterDatabase) ? "" : Path.GetFullPath(paths.MasterDatabase);
        string? resourcesBaseUrl = string.IsNullOrEmpty(paths.ResourcesBaseUrl) ? null : paths.ResourcesBaseUrl;
        if (resourcesBaseUrl != null && !ResourcesUrl.IsValid(resourcesBaseUrl))
        {
            app.Logger.LogWarning("Server:Paths:ResourcesBaseUrl {Url} is ignored: it must be exactly {Length} ASCII characters, http or https, with a path of only 'a's (e.g. http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaaa). Using the address each client reaches the server on instead.",
                resourcesBaseUrl, ResourcesUrl.Length);
            resourcesBaseUrl = null;
        }

        // list.bin of the client's revision, with the CDN base URL pointing here: the configured ResourcesBaseUrl,
        // else the address the client reached this request on (ResourcesUrl.ForHost).
        app.MapGet("/v1/list/300116832/{revision}", async (string revision, HttpContext ctx) =>
        {
            string? path = assetDb.ListPath(revision);
            if (path == null || !File.Exists(path))
            {
                app.Logger.LogWarning("Asset list not found: revision={Revision} path={Path}", revision, path ?? "(not a revision)");
                return Results.NotFound();
            }

            byte[] data = await File.ReadAllBytesAsync(path);
            RewriteResourcesUrl(data, resourcesBaseUrl, ctx.Request, app.Logger);
            return Results.Bytes(data, "application/x-protobuf");
        });

        // Asset Bundles / Resources. Path: /{a…a}/unso-{version}-{type}/{objectId}, the first segment being the run
        // of 'a's that pads the base URL in list.bin to its fixed length (any length, see ResourcesUrl), and type
        // "assetbundle" or "resources".
        app.MapGet("/{padding:regex(^a+$)}/unso-{version}-{type}/{objectId}", (string version, string type, string objectId, HttpContext ctx) =>
        {
            AssetLookup? lookup = assetDb.Find(type, objectId);
            if (lookup == null)
            {
                app.Logger.LogWarning("Asset not in the catalogue: objectId={ObjectId} type={Type}", objectId, type);
                return Results.NotFound();
            }

            ListBinEntry entry = lookup.Entry;
            foreach (string path in lookup.Paths)
            {
                FileInfo info = new(path);
                if (!info.Exists)
                {
                    continue;
                }

                // Sizes under 256 bytes in list.bin aren't reliable, so they aren't checked.
                if (entry.Size >= 256 && info.Length != entry.Size)
                {
                    app.Logger.LogDebug("Asset size mismatch: objectId={ObjectId} path={Path} expected={Expected} actual={Actual} — skipping",
                        objectId, path, entry.Size, info.Length);
                    continue;
                }

                if (!string.IsNullOrEmpty(entry.MD5))
                {
                    string? actualMd5 = assetDb.ComputeMd5(path, info);
                    if (actualMd5 is null || !string.Equals(actualMd5, entry.MD5, StringComparison.OrdinalIgnoreCase))
                    {
                        app.Logger.LogDebug("Asset MD5 mismatch: objectId={ObjectId} path={Path} expected={Expected} actual={Actual} — skipping",
                            objectId, path, entry.MD5, actualMd5);
                        continue;
                    }
                }

                return Results.File(path, "application/octet-stream", enableRangeProcessing: true);
            }

            app.Logger.LogWarning("Asset file not found: objectId={ObjectId} type={Type} name={Name}", objectId, type, entry.Path);
            return Results.NotFound();
        });

        // Master Database. In dev mode the advertised version carries the file's write time after a '_', which
        // isn't part of the file name. Ranges, HEAD and If-None-Match are handled by the file result.
        app.MapMethods("/assets/release/{masterVersion}/database.bin.e", ["GET", "HEAD"], (string masterVersion) =>
        {
            string baseVersion = MasterDataSnapshot.BaseVersion(masterVersion);
            FileInfo file = new(Path.Combine(masterDatabaseBasePath, $"{baseVersion}.bin.e"));
            if (masterDatabaseBasePath.Length == 0 || baseVersion.Length is 0 or > 20 || !baseVersion.All(char.IsAsciiDigit) || !file.Exists)
            {
                app.Logger.LogWarning("Master database not found: version={Version} path={Path}", masterVersion, file.FullName);
                return Results.NotFound();
            }

            EntityTagHeaderValue etag = new($"\"{file.LastWriteTimeUtc.Ticks:x}-{file.Length:x}\"");
            return Results.File(file.FullName, "application/octet-stream", lastModified: file.LastWriteTimeUtc, entityTag: etag, enableRangeProcessing: true);
        });

        return app;
    }

    /// <summary>
    /// Registers the dev-only save point endpoints (<see cref="SavePoints"/>):
    /// <c>/debug/save?name=&amp;user=</c>, <c>/debug/load?name=</c> and <c>/debug/saves</c>; plus
    /// <c>/debug/bridge/transfer</c> and <c>/debug/bridge/link?user=</c>, which open the Bridge pages without the game.
    /// </summary>
    public static WebApplication MapDebugApis(this WebApplication app)
    {
        ServerOptions options = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
        SavePoints savePoints = new(app.Services.GetRequiredService<UserDataStore>(), DataOptions.ResolvePath(options.Data.SavesDirectory));

        app.MapGet("/debug/save", (string? name, long? user) => SavePointCall(() =>
        {
            long userId = savePoints.Save(name, user);
            return $"Saved user {userId} as '{name ?? SavePoints.DefaultName}'.";
        }));

        app.MapGet("/debug/load", (string? name) => SavePointCall(() =>
        {
            long userId = savePoints.Load(name);
            return $"Loaded '{name ?? SavePoints.DefaultName}' into user {userId}. Return to the title screen to see it in game.";
        }));

        app.MapGet("/debug/saves", () => Results.Ok(savePoints.List()));

        // The Bridge pages without the game: a token as GetBackupToken would give one, then the page.
        // /debug/bridge/transfer?device= links a made-up new device; /debug/bridge/link?user= opens Data Linkage.
        app.MapGet("/debug/bridge/transfer", (string? device, Accounts.AccountStore accounts) =>
            Results.Redirect($"/bridge/update?token={accounts.MintTransferToken(device ?? $"debug-{Guid.NewGuid():N}")}"));
        app.MapGet("/debug/bridge/link", (long user, Accounts.AccountStore accounts, UserDataStore store) =>
        {
            string device = store.DevicesOf(user).FirstOrDefault() ?? $"debug-{user}";
            return Results.Redirect($"/bridge/reg?token={accounts.MintRegisterToken(device, user)}");
        });

        return app;
    }

    private static IResult SavePointCall(Func<string> action)
    {
        try
        {
            return Results.Ok(action());
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    /// <summary>
    /// Points the CDN base URL in list.bin at this server: <paramref name="configuredUrl"/> when set (already
    /// checked with <see cref="ResourcesUrl.IsValid"/>), else built from the scheme and host the client used for
    /// <paramref name="request"/>. Logs and leaves the list unmodified when neither fits.
    /// </summary>
    private static void RewriteResourcesUrl(byte[] data, string? configuredUrl, HttpRequest request, ILogger logger)
    {
        string? url = configuredUrl ?? ResourcesUrl.ForHost(request.Scheme, request.Host.ToUriComponent());
        if (url == null)
        {
            logger.LogWarning("Host {Host} is too long for the asset URL in list.bin (at most {Max} characters with http); set Server:Paths:ResourcesBaseUrl. Serving list.bin unmodified.",
                request.Host.ToUriComponent(), ResourcesUrl.Length - "http:///a".Length);
            return;
        }

        if (!ResourcesUrl.Rewrite(data, url))
        {
            logger.LogWarning("Couldn't point list.bin at {Url}: the list doesn't contain the CDN URL. Serving list.bin unmodified.", url);
            return;
        }

        logger.LogDebug("list.bin: rewrote resource base URL to {Url}", url);
    }
}
