using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Http;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Extensions;

/// <summary>
/// Registers all non-gRPC HTTP endpoints: asset serving (list.bin, asset bundles, master database),
/// terms of service, and the catch-all fallback route. Debug save point endpoints are mapped separately
/// by <see cref="MapDebugApis"/> when dev mode allows it.
/// </summary>
public static class HttpApiExtensions
{
    /// <summary>
    /// Registers all HTTP endpoints, including asset serving, master database, and the catch-all route.
    /// </summary>
    public static WebApplication MapHttpApis(this WebApplication app)
    {
        ServerOptions options = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
        string assetDatabaseBasePath = options.Paths.AssetDatabase;
        string masterDatabaseBasePath = options.Paths.MasterDatabase;
        string resourcesBaseUrl = options.Paths.ResourcesBaseUrl;

        ILogger<AssetDatabase> assetLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<AssetDatabase>();
        AssetDatabase assetDb = new(assetDatabaseBasePath, assetLogger);

        app.MapGet("/", () => "Marie's Wonderland is open for business :marie:");

        // ToS. Expects the version wrapped in delimiters like "###123###".
        app.MapGet("/web/static/{languagePath}/terms/termsofuse", (string languagePath) => $"<html><head><title>Terms of Service</title></head><body><h1>Terms of Service</h1><p>Language: {languagePath}</p><p>Version: ###123###</p></body></html>");

        // Asset Database — serves list.bin with the embedded CDN base URL pointing here: the configured
        // ResourcesBaseUrl, else the address the client reached this request on (ResourcesUrl.ForHost).
        // Records which revision the client is using so subsequent asset requests resolve correctly.
        app.MapGet("/v1/list/300116832/{revision}", async (string revision, HttpContext ctx) =>
        {
            string clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
            assetDb.RememberRevision(clientIp, revision);

            byte[] data = await File.ReadAllBytesAsync(Path.Combine(assetDatabaseBasePath, revision, "list.bin"));
            RewriteResourcesUrl(data, resourcesBaseUrl, ctx.Request, app.Logger);
            return Results.Bytes(data, "application/x-protobuf");
        });

        // Asset Bundles / Resources — resolves objectId via the list.bin index for the client's active revision.
        // Path: /{a…a}/unso-{version}-{type}/{objectId}, the first segment being the run of 'a's that pads the base
        // URL in list.bin to its fixed length (any length, see ResourcesUrl).
        //   type = "assetbundle" or "resources" (last segment of "unso-…" after splitting on '-')
        app.MapGet("/{padding:regex(^a+$)}/unso-{version}-{type}/{objectId}", (string version, string type, string objectId, HttpContext ctx) =>
        {
            string clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "";

            foreach (AssetCandidate candidate in assetDb.Resolve(clientIp, type, objectId))
            {
                FileInfo info = new(candidate.Path);
                if (!info.Exists) continue;

                // Size validation: only enforce when list.bin provided a plausible size (≥ 256 bytes).
                if (candidate.ExpectedSize >= 256 && info.Length != candidate.ExpectedSize)
                {
                    app.Logger.LogDebug(
                        "Asset size mismatch: objectId={ObjectId} path={Path} expected={Expected} actual={Actual} — skipping",
                        objectId, candidate.Path, candidate.ExpectedSize, info.Length);
                    continue;
                }

                // MD5 validation when the index provided a checksum.
                if (!string.IsNullOrEmpty(candidate.ExpectedMD5))
                {
                    string? actualMd5 = assetDb.ComputeMd5(candidate.Path, info);
                    if (actualMd5 is not null && !string.Equals(actualMd5, candidate.ExpectedMD5, StringComparison.OrdinalIgnoreCase))
                    {
                        app.Logger.LogDebug(
                            "Asset MD5 mismatch: objectId={ObjectId} path={Path} expected={Expected} actual={Actual} source={Source} — skipping",
                            objectId, candidate.Path, candidate.ExpectedMD5, actualMd5, candidate.Source);
                        continue;
                    }
                }

                // Serve the file — Results.File handles Range requests, ETags, etc.
                return Results.File(candidate.Path, "application/octet-stream");
            }

            app.Logger.LogWarning("Asset not found: objectId={ObjectId} type={Type} clientIp={Ip}", objectId, type, clientIp);
            return Results.NotFound();
        });

        // Master Database. In dev mode the advertised version carries the file's write time, which isn't part of the file name.
        app.MapMethods("/assets/release/{masterVersion}/database.bin.e", ["GET", "HEAD"], async (HttpContext ctx, string masterVersion) =>
        {
            string filePath = Path.Combine(masterDatabaseBasePath, $"{MasterDataSnapshot.BaseVersion(masterVersion)}.bin.e");

            var fileInfo = new FileInfo(filePath);
            long totalLength = fileInfo.Length;

            // Advertise range support
            ctx.Response.Headers.AcceptRanges = "bytes";

            // Simple ETag using last write ticks & length (optional but useful for clients)
            ctx.Response.Headers.ETag = $"\"{fileInfo.LastWriteTimeUtc.Ticks:x}-{totalLength:x}\"";

            // Handle HEAD quickly (send headers only)
            bool isHead = string.Equals(ctx.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase);

            // Parse Range header if present
            if (ctx.Request.Headers.TryGetValue("Range", out var rangeHeader))
            {
                // Expect single range of form: bytes=start-end
                var raw = rangeHeader.ToString();
                if (!raw.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                {
                    // Malformed range; respond with 416
                    ctx.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                    ctx.Response.Headers.ContentRange = $"bytes */{totalLength}";
                    return;
                }

                var rangePart = raw["bytes=".Length..].Trim();
                var parts = rangePart.Split('-', 2);
                if (parts.Length != 2)
                {
                    ctx.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                    ctx.Response.Headers.ContentRange = $"bytes */{totalLength}";
                    return;
                }

                bool startParsed = long.TryParse(parts[0], out long start);
                bool endParsed = long.TryParse(parts[1], out long end);

                if (!startParsed && endParsed)
                {
                    // suffix range: last 'end' bytes
                    long suffixLength = end;
                    if (suffixLength <= 0)
                    {
                        ctx.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                        ctx.Response.Headers.ContentRange = $"bytes */{totalLength}";
                        return;
                    }
                    start = Math.Max(0, totalLength - suffixLength);
                    end = totalLength - 1;
                }
                else if (startParsed && !endParsed)
                {
                    // range from start to end of file
                    end = totalLength - 1;
                }
                else if (!startParsed && !endParsed)
                {
                    ctx.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                    ctx.Response.Headers.ContentRange = $"bytes */{totalLength}";
                    return;
                }

                // Validate
                if (start < 0 || end < start || start >= totalLength)
                {
                    ctx.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                    ctx.Response.Headers.ContentRange = $"bytes */{totalLength}";
                    return;
                }

                long length = end - start + 1;

                ctx.Response.StatusCode = StatusCodes.Status206PartialContent;
                ctx.Response.Headers.ContentRange = $"bytes {start}-{end}/{totalLength}";
                ctx.Response.ContentType = "application/octet-stream";
                ctx.Response.ContentLength = length;

                if (isHead)
                {
                    return;
                }

                // Stream the requested range
                await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Seek(start, SeekOrigin.Begin);

                var buffer = new byte[64 * 1024];
                long remaining = length;
                while (remaining > 0)
                {
                    int toRead = (int)Math.Min(buffer.Length, remaining);
                    int read = await fs.ReadAsync(buffer.AsMemory(0, toRead));
                    if (read == 0) break;
                    await ctx.Response.Body.WriteAsync(buffer.AsMemory(0, read));
                    remaining -= read;
                }

                return;
            }

            // No Range header: return full file
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            ctx.Response.ContentType = "application/octet-stream";
            ctx.Response.ContentLength = totalLength;

            if (isHead)
            {
                return;
            }

            // Stream the whole file
            await using var fullFs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fullFs.CopyToAsync(ctx.Response.Body);
        });

        // Catch all
        app.MapMethods("/{**catchAll}", ["GET", "POST", "PUT", "DELETE", "PATCH",], (HttpContext ctx, string catchAll) =>
        {
            app.Logger.LogWarning("Catchall endpoint hit for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            return $"You requested: {catchAll}";
        });

        return app;
    }

    /// <summary>
    /// Registers the dev-only save point endpoints (<see cref="SavePoints"/>):
    /// <c>/debug/save?name=&amp;user=</c>, <c>/debug/load?name=</c> and <c>/debug/saves</c>.
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
    /// Points the CDN base URL in list.bin at this server: <paramref name="configuredUrl"/> when set (exactly
    /// <see cref="ResourcesUrl.Length"/> characters), else built from the scheme and host the client used for
    /// <paramref name="request"/>. Logs and leaves the list unmodified when neither fits.
    /// </summary>
    private static void RewriteResourcesUrl(byte[] data, string configuredUrl, HttpRequest request, ILogger logger)
    {
        string? url = string.IsNullOrEmpty(configuredUrl)
            ? ResourcesUrl.ForHost(request.Scheme, request.Host.ToUriComponent())
            : configuredUrl;
        if (url == null)
        {
            logger.LogWarning("Host {Host} is too long for the asset URL in list.bin (at most {Max} characters with http); set Server:Paths:ResourcesBaseUrl. Serving list.bin unmodified.",
                request.Host.ToUriComponent(), ResourcesUrl.Length - "http:///a".Length);
            return;
        }

        if (!ResourcesUrl.Rewrite(data, url))
        {
            logger.LogWarning("Couldn't point list.bin at {Url}: it must be exactly {Length} ASCII characters and the list must contain the CDN URL. Serving list.bin unmodified.",
                url, ResourcesUrl.Length);
            return;
        }

        logger.LogDebug("list.bin: rewrote resource base URL to {Url}", url);
    }
}
