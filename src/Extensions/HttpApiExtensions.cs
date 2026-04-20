using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Http;
using Microsoft.Extensions.Options;
using System.Text;

namespace MariesWonderland.Extensions;

/// <summary>
/// Registers all non-gRPC HTTP endpoints: asset serving (list.bin, asset bundles, master database),
/// debug save/load utilities, terms of service, and the catch-all fallback route.
/// </summary>
public static class HttpApiExtensions
{
    // The URL embedded in list.bin pointing to the original CDN (must be exactly 43 ASCII bytes).
    private static readonly byte[] ResourcesUrlOriginal =
        Encoding.ASCII.GetBytes("https://resources.app.nierreincarnation.com");

    /// <summary>
    /// Registers all HTTP endpoints, including asset serving, master database, debug utilities, and the catch-all route.
    /// </summary>
    public static WebApplication MapHttpApis(this WebApplication app)
    {
        ServerOptions options = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
        string assetDatabaseBasePath = options.Paths.AssetDatabase;
        string masterDatabaseBasePath = options.Paths.MasterDatabase;
        string resourcesBaseUrl = options.Paths.ResourcesBaseUrl;
        string userDatabasePath = Path.IsPathRooted(options.Data.UserDatabase)
            ? options.Data.UserDatabase
            : Path.Combine(AppContext.BaseDirectory, options.Data.UserDatabase);

        ILogger<AssetDatabase> assetLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<AssetDatabase>();
        AssetDatabase assetDb = new(assetDatabaseBasePath, assetLogger);

        app.MapGet("/", () => "Marie's Wonderland is open for business :marie:");

        // Debug endpoints for manual save/load
        app.MapGet("/debug/save", () =>
        {
            try
            {
                UserDataStore store = app.Services.GetRequiredService<UserDataStore>();
                store.Save(userDatabasePath);
                return Results.Ok($"Saved {store.All.Count} users to {userDatabasePath}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Save failed: {ex.Message}");
            }
        });

        app.MapGet("/debug/load", () =>
        {
            try
            {
                UserDataStore store = app.Services.GetRequiredService<UserDataStore>();
                int count = store.Load(userDatabasePath);
                return count > 0
                    ? Results.Ok($"Loaded {count} users from {userDatabasePath}")
                    : Results.Ok("No save file found.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Load failed: {ex.Message}");
            }
        });

        // ToS. Expects the version wrapped in delimiters like "###123###".
        app.MapGet("/web/static/{languagePath}/terms/termsofuse", (string languagePath) => $"<html><head><title>Terms of Service</title></head><body><h1>Terms of Service</h1><p>Language: {languagePath}</p><p>Version: ###123###</p></body></html>");

        // Asset Database — serves list.bin, rewriting the embedded CDN base URL if configured.
        // Records which revision the client is using so subsequent asset requests resolve correctly.
        app.MapGet("/v1/list/300116832/{revision}", async (string revision, HttpContext ctx) =>
        {
            string clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
            assetDb.RememberRevision(clientIp, revision);

            byte[] data = await File.ReadAllBytesAsync(Path.Combine(assetDatabaseBasePath, revision, "list.bin"));
            RewriteResourcesUrl(data, resourcesBaseUrl, app.Logger);
            return Results.Bytes(data, "application/x-protobuf");
        });

        // Asset Bundles / Resources — resolves objectId via the list.bin index for the client's active revision.
        // Path: /aaaaaaaaaaaaaaaaaaaaaaaa/unso-{version}-{type}/{objectId}
        //   type = "assetbundle" or "resources" (last segment of "unso-…" after splitting on '-')
        app.MapGet("/aaaaaaaaaaaaaaaaaaaaaaaa/unso-{version}-{type}/{objectId}", (string version, string type, string objectId, HttpContext ctx) =>
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

        // Master Database
        app.MapMethods("/assets/release/{masterVersion}/database.bin.e", ["GET", "HEAD"], async (HttpContext ctx, string masterVersion) =>
        {
            var filePath = Path.Combine(masterDatabaseBasePath, $"{masterVersion}.bin.e");

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
    /// Rewrites the CDN base URL embedded in list.bin bytes to the configured local URL.
    /// The replacement must be exactly 43 ASCII bytes to match the original protobuf field length.
    /// </summary>
    private static void RewriteResourcesUrl(byte[] data, string replacementUrl, ILogger logger)
    {
        if (string.IsNullOrEmpty(replacementUrl))
            return;

        byte[] replacement = Encoding.ASCII.GetBytes(replacementUrl);
        if (replacement.Length != ResourcesUrlOriginal.Length)
        {
            logger.LogWarning(
                "ResourcesBaseUrl is {Length} bytes but must be exactly {Required} bytes — serving list.bin unmodified.",
                replacement.Length, ResourcesUrlOriginal.Length);
            return;
        }

        int idx = data.AsSpan().IndexOf(ResourcesUrlOriginal);
        if (idx < 0)
        {
            logger.LogWarning("CDN URL not found in list.bin — serving unmodified.");
            return;
        }

        replacement.CopyTo(data, idx);
        logger.LogDebug("list.bin: rewrote resource base URL to {Url}", replacementUrl);
    }
}
