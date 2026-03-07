using Art.Framework.ApiNetwork.Grpc.Api.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class DataService : Art.Framework.ApiNetwork.Grpc.Api.Data.DataService.DataServiceBase
{
    private const string LatestMasterDataVersion = "20240404193219";
    private const string UserDataBasePath = @"path\to\user\data";

    private const string TablePrefix = "Entity";
    private const string TableSuffix = "Table";

    public override Task<MasterDataGetLatestVersionResponse> GetLatestMasterDataVersion(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new MasterDataGetLatestVersionResponse
        {
            LatestMasterDataVersion = LatestMasterDataVersion
        });
    }

    public override Task<UserDataGetNameResponseV2> GetUserDataNameV2(Empty request, ServerCallContext context)
    {
        UserDataGetNameResponseV2 response = new();
        TableNameList tableNameList = new();
        var names = Directory
            .EnumerateFiles(UserDataBasePath, "*.json")
            .Select(path =>
            {
                var name = Path.GetFileNameWithoutExtension(path); // e.g. "EntityIUserTable"
                return name.Substring(TablePrefix.Length, name.Length - TablePrefix.Length - TableSuffix.Length); // result for "EntityIUserTable" -> "IUser"
            });

        tableNameList.TableName.AddRange(names);
        response.TableNameList.Add(tableNameList);

        return Task.FromResult(response);
    }

    public override Task<UserDataGetResponse> GetUserData(UserDataGetRequest request, ServerCallContext context)
    {
        UserDataGetResponse response = new();

        foreach (var tableName in request.TableName)
        {
            var filePath = Path.Combine(UserDataBasePath, TablePrefix + tableName + TableSuffix + ".json");
            var jsonContent = File.ReadAllText(filePath);
            response.UserDataJson.Add(tableName, jsonContent);
        }

        return Task.FromResult(response);
    }
}
