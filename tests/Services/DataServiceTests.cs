using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Data;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using DataServiceImpl = MariesWonderland.Services.DataService;

namespace MariesWonderland.Tests.Services;

public class DataServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long UserId = 1L;

    private DataServiceImpl CreateService(UserDataStore store, string latestMasterDataVersion = "")
    {
        ServerOptions options = new()
        {
            Data = new DataOptions
            {
                LatestMasterDataVersion = latestMasterDataVersion
            }
        };

        return new DataServiceImpl(Options.Create(options), store);
    }

    private static FakeServerCallContext ContextFor(long userId, string? sessionKey = null)
        => FakeServerCallContext.For(userId, sessionKey);

    [Fact]
    public async Task GetLatestMasterDataVersion_ReturnsConfiguredVersion()
    {
        UserDataStore store = new(MasterDb);
        DataServiceImpl service = CreateService(store, latestMasterDataVersion: "2025.04.01");

        MasterDataGetLatestVersionResponse response = await service.GetLatestMasterDataVersion(
            new Empty(),
            ContextFor(UserId));

        Assert.Equal("2025.04.01", response.LatestMasterDataVersion);
    }

    [Fact]
    public async Task GetUserDataNameV2_ReturnsSortedUserDataTableNames()
    {
        UserDataStore store = new(MasterDb);
        DataServiceImpl service = CreateService(store);

        UserDataGetNameResponseV2 response = await service.GetUserDataNameV2(
            new Empty(),
            ContextFor(UserId));

        TableNameList tableNameList = Assert.Single(response.TableNameList);
        Assert.Equal(UserDataDiffBuilder.TableNames.Order(), tableNameList.TableName);
    }

    [Fact]
    public async Task GetUserData_WithValidSession_ReturnsRequestedTables()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserProfile.Add(new EntityIUserProfile
        {
            UserId = UserId,
            Name = "Marie",
            Message = "Welcome"
        });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = UserId,
            Level = 10,
            Exp = 250
        });

        UserDataStore store = CreateStore(UserId, userDb, MasterDb);
        UserSession session = store.CreateSession(UserId, TimeSpan.FromMinutes(5));
        DataServiceImpl service = CreateService(store);
        UserDataGetRequest request = new();
        request.TableName.Add("IUserStatus");
        request.TableName.Add("IUserProfile");
        request.TableName.Add("UnknownTable");

        UserDataGetResponse response = await service.GetUserData(
            request,
            ContextFor(UserId, session.SessionKey));

        Assert.Equal(UserDataDiffBuilder.SerializeTable(userDb, "IUserStatus"), response.UserDataJson["IUserStatus"]);
        Assert.Equal(UserDataDiffBuilder.SerializeTable(userDb, "IUserProfile"), response.UserDataJson["IUserProfile"]);
        Assert.Equal("[]", response.UserDataJson["UnknownTable"]);
    }

    [Fact]
    public async Task GetUserData_WithoutAuthenticatedSession_ThrowsUnauthenticated()
    {
        UserDataStore store = CreateStore(UserId, CreateUserDb(), MasterDb);
        DataServiceImpl service = CreateService(store);

        RpcException exception = await Assert.ThrowsAsync<RpcException>(() => service.GetUserData(
            new UserDataGetRequest(),
            FakeServerCallContext.Anonymous()));

        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
    }
}
