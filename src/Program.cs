using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Interceptors;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace MariesWonderland;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(80, x => x.Protocols = HttpProtocols.Http1AndHttp2);
            options.ListenAnyIP(443, x =>
            {
                //var cert = X509CertificateLoader.LoadPkcs12FromFile(@"E:\Downloads\cert_key\cert.pfx", "yourpassword");
                //x.UseHttps(cert);
                x.Protocols = HttpProtocols.Http2;
            });
        });

        builder.Services.AddSingleton<CommonHeaderInterceptor>();
        builder.Services.AddSingleton<DiffInterceptor>();
        builder.Services.AddSingleton<LoggingInterceptor>();
        builder.Services.AddSingleton<AutoSaveInterceptor>();
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<LoggingInterceptor>();
            options.Interceptors.Add<DiffInterceptor>();
            options.Interceptors.Add<CommonHeaderInterceptor>();
            options.Interceptors.Add<AutoSaveInterceptor>();
        });
        builder.Services.AddServerOptions(builder.Configuration);
        builder.Services.AddDataStores(builder.Configuration);

        var app = builder.Build();

        // Load user data on startup
        UserDataStore userDataStore = app.Services.GetRequiredService<UserDataStore>();
        ServerOptions serverOptions = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
        string userDatabasePath = Path.IsPathRooted(serverOptions.Data.UserDatabase)
            ? serverOptions.Data.UserDatabase
            : Path.Combine(AppContext.BaseDirectory, serverOptions.Data.UserDatabase);

        int loadedUsers = userDataStore.Load(userDatabasePath);
        if (loadedUsers > 0)
        {
            app.Logger.LogInformation("Loaded {Count} users from {Path}", loadedUsers, userDatabasePath);
        }

        app.MapGrpcServices();
        app.MapHttpApis();

        app.Run();
    }
}