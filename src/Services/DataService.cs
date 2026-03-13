using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Proto.Data;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Services;

public class DataService(IOptions<ServerOptions> options, UserDataStore userDataStore)
    : MariesWonderland.Proto.Data.DataService.DataServiceBase
{
    private readonly DataOptions _data = options.Value.Data;

    public override Task<MasterDataGetLatestVersionResponse> GetLatestMasterDataVersion(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new MasterDataGetLatestVersionResponse
        {
            LatestMasterDataVersion = _data.LatestMasterDataVersion
        });
    }

    public override Task<UserDataGetNameResponseV2> GetUserDataNameV2(Empty request, ServerCallContext context)
    {
        TableNameList tableNameList = new();
        tableNameList.TableName.AddRange(UserDataDiffBuilder.TableNames.Order());

        UserDataGetNameResponseV2 response = new();
        response.TableNameList.Add(tableNameList);

        return Task.FromResult(response);
    }

    public override Task<UserDataGetResponse> GetUserData(UserDataGetRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        string sessionKey = context.GetSessionKey();

        if (!userDataStore.TryResolveSession(sessionKey, out long resolvedUserId) || resolvedUserId != userId)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired session."));
        }

        DarkUserMemoryDatabase userDb = userDataStore.GetOrCreate(userId);
        UserDataGetResponse response = new();

        foreach (string tableName in request.TableName)
        {
            response.UserDataJson[tableName] = UserDataDiffBuilder.SerializeTable(userDb, tableName);
        }

        return Task.FromResult(response);
    }
}
