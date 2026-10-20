using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Proto.Data;

namespace MariesWonderland.Services;

public class DataService(MasterDataSnapshot masterData, UserDataStore userDataStore)
    : MariesWonderland.Proto.Data.DataService.DataServiceBase
{
    /// <summary>Returns the latest master data version so the client can check if an asset update is needed.</summary>
    public override Task<MasterDataGetLatestVersionResponse> GetLatestMasterDataVersion(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new MasterDataGetLatestVersionResponse
        {
            LatestMasterDataVersion = masterData.Version
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

    /// <summary>Returns the full user state for the requested tables, serialized as JSON.</summary>
    public override Task<UserDataGetResponse> GetUserData(UserDataGetRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = userDataStore.GetOrCreate(userId);
        UserDataGetResponse response = new();

        foreach (string tableName in request.TableName)
        {
            response.UserDataJson[tableName] = UserDataDiffBuilder.SerializeTable(userDb, tableName);
        }

        return Task.FromResult(response);
    }
}
