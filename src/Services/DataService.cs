using Art.Framework.ApiNetwork.Grpc.Api.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class DataService : Art.Framework.ApiNetwork.Grpc.Api.Data.DataService.DataServiceBase
{
    private const string LatestMasterDataVersion = "20240404193219";
    private const string UserDataBasePath = @"path\to\user\data";

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
        tableNameList.TableName.AddRange(Directory.EnumerateFiles(UserDataBasePath, "*.json").Select(x => Path.GetFileNameWithoutExtension(x)));
        response.TableNameList.Add(tableNameList);

        return Task.FromResult(response);
    }

    public override Task<UserDataGetResponse> GetUserData(UserDataGetRequest request, ServerCallContext context)
    {
        UserDataGetResponse response = new();

        foreach (var tableName in request.TableName)
        {
            var filePath = Path.Combine(UserDataBasePath, tableName + ".json");
            var jsonContent = File.ReadAllText(filePath);
            response.UserDataJson.Add(tableName, jsonContent);
        }

        return Task.FromResult(response);
    }
}
