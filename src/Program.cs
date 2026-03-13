using MariesWonderland.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

        builder.Services.AddGrpc();
        builder.Services.AddServerOptions(builder.Configuration);

        var app = builder.Build();

        app.MapGrpcServices();
        app.MapHttpApis();

        app.Run();
    }
}