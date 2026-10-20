using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Interceptors;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MariesWonderland.Tests.Interceptors;

public class DiffInterceptorTests : InterceptorTestBase
{
    private static readonly ILogger<DiffInterceptor> Logger = NullLogger<DiffInterceptor>.Instance;

    private static DiffInterceptor CreateInterceptor(UserDataStore store) =>
        new(store, Logger);

    private record NoopResponse;

    /// <summary>
    /// Verifies that when the response type has no DiffUserData property,
    /// the interceptor passes through without modification and returns the
    /// continuation's result unchanged.
    /// </summary>
    [Fact]
    public async Task ResponseWithoutDiffProperty_PassesThroughWithoutModification()
    {
        var store = CreateEmptyStore();
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 1);
        var expected = new NoopResponse();

        var result = await CallInterceptor(
            interceptor,
            new object(),
            context,
            () => expected);

        Assert.Same(expected, result);
    }

    /// <summary>
    /// Verifies that when the user's database is not modified during the call,
    /// the DiffUserData map on the response remains empty and no
    /// x-apb-update-user-data-names trailer is added.
    /// </summary>
    [Fact]
    public async Task KnownUser_DbUnchanged_DiffIsEmpty_NoTrailer()
    {
        var userDb = new DarkUserMemoryDatabase();
        var store = CreateStoreWithUser(1, userDb);
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 1);

        var response = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () => new SetUserNameResponse());

        Assert.Empty(response.DiffUserData);

        var trailer = context.ResponseTrailers
            .FirstOrDefault(m => m.Key == "x-apb-update-user-data-names");
        Assert.Null(trailer);
    }

    /// <summary>
    /// Verifies that when the continuation modifies the user's database
    /// (adds a weapon record), the DiffUserData map on the response
    /// contains the corresponding "IUserWeapon" key.
    /// </summary>
    [Fact]
    public async Task KnownUser_DbModified_DiffPopulated()
    {
        var userDb = new DarkUserMemoryDatabase();
        var store = CreateStoreWithUser(1, userDb);
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 1);

        var response = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () =>
            {
                userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
                {
                    UserId = 1,
                    UserWeaponUuid = "test-uuid",
                    WeaponId = 100,
                    Level = 1
                });
                return new SetUserNameResponse();
            });

        Assert.Contains("IUserWeapon", response.DiffUserData.Keys);
    }

    /// <summary>
    /// Verifies that when the continuation modifies two different tables,
    /// the x-apb-update-user-data-names trailer contains both table names
    /// comma-separated and sorted alphabetically.
    /// </summary>
    [Fact]
    public async Task KnownUser_DbModified_TrailerContainsSortedTableNames()
    {
        var userDb = new DarkUserMemoryDatabase();
        var store = CreateStoreWithUser(1, userDb);
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 1);

        var response = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () =>
            {
                userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
                {
                    UserId = 1,
                    UserWeaponUuid = "test-uuid",
                    WeaponId = 100,
                    Level = 1
                });
                userDb.EntityIUserStatus.Add(new EntityIUserStatus
                {
                    UserId = 1,
                    Level = 1,
                    Exp = 0,
                    StaminaMilliValue = 60000,
                    StaminaUpdateDatetime = 0
                });
                return new SetUserNameResponse();
            });

        var trailer = context.ResponseTrailers
            .FirstOrDefault(m => m.Key == "x-apb-update-user-data-names");
        Assert.NotNull(trailer);

        // "IUserStatus" comes before "IUserWeapon" alphabetically
        Assert.Equal("IUserStatus,IUserWeapon", trailer.Value);
    }

    /// <summary>
    /// Verifies that calls without a session (RegisterUser, Auth, TransferUser…) get no diff,
    /// even when the response names a user that exists in the store.
    /// </summary>
    [Fact]
    public async Task PreLoginCall_NoDiff()
    {
        DarkUserMemoryDatabase userDb = new();
        userDb.EntityIUser.Add(new EntityIUser
        {
            UserId = 42,
            PlayerId = 1,
            RegisterDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        UserDataStore store = CreateStoreWithUser(42, userDb);
        DiffInterceptor interceptor = CreateInterceptor(store);
        FakeServerCallContext context = FakeServerCallContext.Anonymous();

        AuthUserResponse response = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () => new AuthUserResponse { UserId = 42 });

        Assert.Empty(response.DiffUserData);
        Assert.DoesNotContain(context.ResponseTrailers, m => m.Key == "x-apb-update-user-data-names");
    }

    /// <summary>
    /// Verifies that when the user is not in the store, the interceptor
    /// gracefully handles it: DiffUserData remains empty and no exception is thrown.
    /// </summary>
    [Fact]
    public async Task UserNotInStore_NoSnapshotTaken_DiffIsEmpty()
    {
        var store = CreateEmptyStore();
        var interceptor = CreateInterceptor(store);
        var context = FakeServerCallContext.For(userId: 99);

        var response = await CallInterceptor(
            interceptor,
            new SetUserNameRequest(),
            context,
            () => new SetUserNameResponse());

        Assert.Empty(response.DiffUserData);
    }
}
