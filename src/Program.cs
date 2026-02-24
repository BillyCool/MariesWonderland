using MariesWonderland.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MariesWonderland;

public static class Program
{
    // https://archive.org/download/nier_reincarnation_assets/resource_dump_android.7z
    private const string AssetDatabaseBasePath = @"path\to\resource_dump_android\revisions";

    // https://archive.org/download/nierreincarnation/Global/master_data/bin/
    private const string MasterDatabaseBasePath = @"path\to\master_data\bin";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Enforce HTTP/2
        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(7777, x => x.Protocols = HttpProtocols.Http2));
        builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(7776, x =>
        {
            x.UseHttps();
            x.Protocols = HttpProtocols.Http2;
        }));

        // Add GRPC
        builder.Services.AddGrpc();

        var app = builder.Build();

        // Add GRPC services
        app.MapGrpcService<GreeterService>();
        app.MapGrpcService<UserService>();
        app.MapGrpcService<DataService>();
        app.MapGrpcService<ConfigService>();
        app.MapGrpcService<GameplayService>();

        // Add HTTP middleware
        app.MapGet("/", () => "Marie's Wonderland is open for business :marie:");

        // ToS. Expects the version wrapped in delimiters like "###123###".
        app.MapGet("/web/static/{languagePath}/terms/termsofuse", (string languagePath) => $"<html><head><title>Terms of Service</title></head><body><h1>Terms of Service</h1><p>Language: {languagePath}</p><p>Version: ###123###</p></body></html>");

        // Asset Database
        app.MapGet("/v2/pub/a/301/v/300116832/list/{revision}", async (string revision) =>
            await File.ReadAllBytesAsync(Path.Combine(AssetDatabaseBasePath, revision, "list.bin")));

        // Master Database
        app.MapMethods("/assets/release/{masterVersion}/database.bin.e", ["GET", "HEAD"], async (HttpContext ctx, string masterVersion) =>
        {
            var filePath = Path.Combine(MasterDatabaseBasePath, $"{masterVersion}.bin.e");

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
                    int read = await fs.ReadAsync(buffer, 0, toRead);
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
        app.MapMethods("/{**catchAll}", ["GET", "POST", "PUT", "DELETE", "PATCH",], (string catchAll) => $"You requested: {catchAll}");

        app.Run();
    }
}