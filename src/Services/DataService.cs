using Art.Framework.ApiNetwork.Grpc.Api.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class DataService : Art.Framework.ApiNetwork.Grpc.Api.Data.DataService.DataServiceBase
{
    public override Task<MasterDataGetLatestVersionResponse> GetLatestMasterDataVersion(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new MasterDataGetLatestVersionResponse());
    }

    public override Task<UserDataGetResponse> GetUserData(UserDataGetRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UserDataGetResponse());
    }
}
