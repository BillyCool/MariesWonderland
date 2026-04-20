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

    /// <summary>Returns the latest master data version so the client can check if an asset update is needed.</summary>
    public override Task<MasterDataGetLatestVersionResponse> GetLatestMasterDataVersion(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new MasterDataGetLatestVersionResponse
        {
            LatestMasterDataVersion = _data.LatestMasterDataVersion
        });
    }

    /// <summary>Returns the sorted list of user data table names used for diff-based synchronization.</summary>
    public override Task<UserDataGetNameResponseV2> GetUserDataNameV2(Empty request, ServerCallContext context)
    {
        TableNameList tableNameList = new();
        tableNameList.TableName.AddRange(UserDataDiffBuilder.TableNames.Order());

        UserDataGetNameResponseV2 response = new();
        response.TableNameList.Add(tableNameList);

        return Task.FromResult(response);
    }

    /// <summary>Returns the full user state for the requested tables, serialized as JSON. Validates session before responding.</summary>
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
