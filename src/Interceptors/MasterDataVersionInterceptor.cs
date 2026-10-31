using MariesWonderland.Constants;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;

namespace MariesWonderland.Interceptors;

/// <summary>
/// gRPC interceptor that sends a player back to the title screen when their client runs on other master data than
/// the server (the dev-mode hot reload gives each edit a new version): code <see cref="NetworkCode.MasterDataUpdated"/>
/// with a Title transition, after which the client re-checks the version and downloads the new data.
/// The client sends the version it loaded in <c>x-apb-master-data-hash</c>. Only calls with a resolved session are
/// checked, and <c>DataService</c> is exempt, so the title flow itself (login, version check, download) can't loop.
/// </summary>
public class MasterDataVersionInterceptor(MasterDataHolder masterData, ILogger<MasterDataVersionInterceptor> logger) : Interceptor
{
    private const string DataServicePrefix = "/apb.api.data.DataService/";

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        string clientVersion = context.RequestHeaders.GetValue("x-apb-master-data-hash") ?? "";
        string serverVersion = masterData.Current.Version;

        if (clientVersion.Length > 0
            && clientVersion != serverVersion
            && context.TryGetUserId(out _)
            && !context.Method.StartsWith(DataServicePrefix, StringComparison.Ordinal))
        {
            logger.LogInformation("{Method}: client has master data {Client}, server has {Server}; sending it to the title", context.Method, clientVersion, serverVersion);
            throw GameError.Refuse(SystemErrors.MasterDataOutOfDate, NetworkCode.MasterDataUpdated, ScreenTransition.Title);
        }

        return continuation(request, context);
    }
}
