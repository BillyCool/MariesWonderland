using MariesWonderland.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MariesWonderland;

public static class Program
{
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
        app.MapGet("/web/static/{languagePath}/terms/termsofuse", (string languagePath) => $"<html><head><title>Terms of Service</title></head><body><h1>Terms of Service</h1><p>Language: {languagePath}</p><p>Version: ###123###</p></body></html>"); // Expects the version wrapped in delimiters like "###123###".
        app.MapGet("/{**catchAll}", (string catchAll) => $"You requested: {catchAll}");
        app.MapPost("/{**catchAll}", (string catchAll) => $"You requested: {catchAll}");
        app.MapPut("/{**catchAll}", (string catchAll) => $"You requested: {catchAll}");
        app.MapDelete("/{**catchAll}", (string catchAll) => $"You requested: {catchAll}");
        app.MapPatch("/{**catchAll}", (string catchAll) => $"You requested: {catchAll}");

        app.Run();
    }
}