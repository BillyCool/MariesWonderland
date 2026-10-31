using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Interceptors;
using MariesWonderland.Middleware;
using MariesWonderland.Persistence;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;

namespace MariesWonderland;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Machine-specific settings (data paths, LAN address); git-ignored. Goes right after the appsettings files so it
        // overrides them, while environment variables and the command line still override it.
        IList<IConfigurationSource> sources = ((IConfigurationBuilder)builder.Configuration).Sources;
        int afterAppSettings = sources
            .Select((source, index) => source is JsonConfigurationSource { Path: string path } && path.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase) ? index + 1 : 0)
            .Max();
        sources.Insert(afterAppSettings, new JsonConfigurationSource { Path = "appsettings.Local.json", Optional = true, ReloadOnChange = true });

        // Listening addresses, protocols and certificates come from the "Kestrel:Endpoints" config section.

        DevModeOptions devMode = builder.Configuration.GetSection(DevModeOptions.SectionName).Get<DevModeOptions>() ?? new();
        builder.Services.Configure<DevModeOptions>(builder.Configuration.GetSection(DevModeOptions.SectionName));

        builder.Services.AddSingleton<SessionInterceptor>();
        builder.Services.AddSingleton<MasterDataVersionInterceptor>();
        builder.Services.AddSingleton<UserLockInterceptor>();
        builder.Services.AddSingleton<CommonHeaderInterceptor>();
        builder.Services.AddSingleton<DiffInterceptor>();
        builder.Services.AddSingleton<LoggingInterceptor>();
        builder.Services.AddSingleton<RequestJournalInterceptor>();
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<SessionInterceptor>();
            options.Interceptors.Add<MasterDataVersionInterceptor>();
            options.Interceptors.Add<UserLockInterceptor>();
            // Outside DiffInterceptor, which hands it each call's changes; inside the lock.
            if (devMode.IsRequestJournalEnabled)
            {
                options.Interceptors.Add<RequestJournalInterceptor>();
            }
            options.Interceptors.Add<LoggingInterceptor>();
            options.Interceptors.Add<DiffInterceptor>();
            options.Interceptors.Add<CommonHeaderInterceptor>();
        });
        builder.Services.AddServerOptions(builder.Configuration);
        builder.Services.AddDataStores(builder.Configuration, devMode);

        var app = builder.Build();

        // Bring the user database up to date before anything reads it (the store loads shared prize claims and friends).
        ServerOptions serverOptions = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
        EfUserPersistence userPersistence = app.Services.GetRequiredService<EfUserPersistence>();
        userPersistence.Migrate();
        UserDataStore userDataStore = app.Services.GetRequiredService<UserDataStore>();
        app.Logger.LogInformation("User database: {Path}", DataOptions.ResolvePath(serverOptions.Data.SqliteDatabase));

        // First start on the database: bring over the users of the old JSON snapshot, if there is one.
        string userSnapshotPath = DataOptions.ResolvePath(serverOptions.Data.UserDatabase);
        if (userPersistence.UserIds().Count == 0 && File.Exists(userSnapshotPath))
        {
            int importedUsers = userDataStore.Load(userSnapshotPath);
            app.Logger.LogInformation("Imported {Count} users from {Path} into the user database", importedUsers, userSnapshotPath);
        }

        // First in the pipeline for gRPC calls: everything behind it, interceptors included, deals with plain messages.
        if (serverOptions.Grpc.EncryptMessages)
        {
            app.UseMiddleware<GrpcPayloadEncryptionMiddleware>();
        }

        app.MapGrpcServices();
        app.MapHttpApis();
        if (devMode.IsDebugEndpointsEnabled)
        {
            app.MapDebugApis();
        }

        if (devMode.Enabled)
        {
            app.Logger.LogWarning("DevMode is enabled. Don't use this configuration on a hosted server.");
        }

        app.Run();
    }
}