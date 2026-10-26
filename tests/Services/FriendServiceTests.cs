using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Friend;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using FriendServiceImpl = MariesWonderland.Services.FriendService;
using NotificationServiceImpl = MariesWonderland.Services.NotificationService;
using UserServiceImpl = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Services;

/// <summary>Friends end to end: search, requests, the friend list, cheers and recommendations.</summary>
public class FriendServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long Alice = 1001;
    private const long Bob = 1002;
    private const long Carol = 1003;

    private static long PlayerOf(long userId) => userId * 10;

    // 2026-10-24 12:00 UTC: four hours into the game day (it turns over at 08:00 UTC).
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 24, 12, 0, 0, TimeSpan.Zero));
    private readonly UserDataStore _store = new(TimeProvider.System);

    private long NowMs => _time.GetUtcNow().ToUnixTimeMilliseconds();

    private FriendServiceImpl Service() => new(_store, MasterDb, GameConfig, TestServices.Missions(MasterDb), _time);

    /// <summary>A player with a name, level, 10 stamina, a quest deck power and a login.</summary>
    private DarkUserMemoryDatabase AddPlayer(long userId, string name, int level = 10, long? lastLoginMs = null)
    {
        DarkUserMemoryDatabase userDb = new() { UserId = userId };
        userDb.EntityIUser.Add(new EntityIUser { UserId = userId, PlayerId = PlayerOf(userId) });
        userDb.EntityIUserProfile.Add(new EntityIUserProfile { UserId = userId, Name = name, FavoriteCostumeId = 10100 });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus { UserId = userId, Level = level, StaminaMilliValue = 10_000, StaminaUpdateDatetime = NowMs });
        userDb.EntityIUserLogin.Add(new EntityIUserLogin { UserId = userId, LastLoginDatetime = lastLoginMs ?? NowMs });
        userDb.EntityIUserDeckTypeNote.Add(new EntityIUserDeckTypeNote { UserId = userId, DeckType = Models.Type.DeckType.QUEST, MaxDeckPower = 1234 });
        _store.Set(userId, userDb);
        return userDb;
    }

    private async Task BefriendAsync(long a, long b)
    {
        FriendServiceImpl service = Service();
        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(b) }, ContextFor(a));
        await service.AcceptFriendRequest(new AcceptFriendRequestRequest { PlayerId = PlayerOf(a) }, ContextFor(b));
    }

    private static int Stamina(DarkUserMemoryDatabase userDb) => userDb.EntityIUserStatus[0].StaminaMilliValue;

    [Fact]
    public async Task GetUser_FindsAPlayerById_AndUnknownIdsReturnNoUser()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob", level: 42);
        FriendServiceImpl service = Service();

        GetUserResponse found = await service.GetUser(new GetUserRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));
        GetUserResponse missing = await service.GetUser(new GetUserRequest { PlayerId = 999 }, ContextFor(Alice));

        Assert.Equal(PlayerOf(Bob), found.User.PlayerId);
        Assert.Equal("Bob", found.User.UserName);
        Assert.Equal(42, found.User.Level);
        Assert.Equal(1234, found.User.MaxDeckPower);
        Assert.Equal(10100, found.User.FavoriteCostumeId);
        Assert.Null(missing.User);
    }

    [Fact]
    public async Task Request_ShowsAsPending_AndAcceptingMakesBothFriends()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        FriendServiceImpl service = Service();
        NotificationServiceImpl notifications = new(_store, _time);

        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));

        GetFriendRequestListResponse pending = await service.GetFriendRequestList(new Empty(), ContextFor(Bob));
        Assert.Equal([PlayerOf(Alice)], pending.User.Select(u => u.PlayerId));
        Assert.Equal(1, (await notifications.GetHeaderNotification(new Empty(), ContextFor(Bob))).FriendRequestReceiveCount);
        Assert.Empty((await service.GetFriendRequestList(new Empty(), ContextFor(Alice))).User);

        await service.AcceptFriendRequest(new AcceptFriendRequestRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Bob));

        Assert.Equal([PlayerOf(Bob)], (await service.GetFriendList(new GetFriendListRequest(), ContextFor(Alice))).FriendUser.Select(f => f.PlayerId));
        Assert.Equal([PlayerOf(Alice)], (await service.GetFriendList(new GetFriendListRequest(), ContextFor(Bob))).FriendUser.Select(f => f.PlayerId));
        Assert.Empty((await service.GetFriendRequestList(new Empty(), ContextFor(Bob))).User);
        Assert.Equal(0, (await notifications.GetHeaderNotification(new Empty(), ContextFor(Bob))).FriendRequestReceiveCount);
    }

    [Fact]
    public async Task CrossingRequests_MakeFriends()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        FriendServiceImpl service = Service();

        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));
        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Bob));

        Assert.True(_store.Friends.AreFriends(Alice, Bob));
        Assert.Empty((await service.GetFriendRequestList(new Empty(), ContextFor(Alice))).User);
    }

    [Fact]
    public async Task RejectAll_DeclinesEveryPendingRequest()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        AddPlayer(Carol, "Carol");
        FriendServiceImpl service = Service();
        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Bob));
        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Carol));

        await service.DeclineFriendRequest(new DeclineFriendRequestRequest { PlayerId = { PlayerOf(Bob), PlayerOf(Carol), 999 } }, ContextFor(Alice));

        Assert.Empty((await service.GetFriendRequestList(new Empty(), ContextFor(Alice))).User);
        Assert.Empty(_store.Friends.FriendsOf(Alice));
    }

    [Fact]
    public async Task PendingRequest_IsWithdrawnAfterSevenDays()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        FriendServiceImpl service = Service();
        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));

        _time.Advance(TimeSpan.FromDays(7));

        Assert.Empty((await service.GetFriendRequestList(new Empty(), ContextFor(Bob))).User);
        RpcException error = await Assert.ThrowsAsync<RpcException>(() =>
            service.AcceptFriendRequest(new AcceptFriendRequestRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Bob)));
        Assert.Equal(StatusCode.FailedPrecondition, error.StatusCode);
    }

    [Fact]
    public async Task RequestToAFriendOrAnUnknownPlayer_IsRefused()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        await BefriendAsync(Alice, Bob);
        FriendServiceImpl service = Service();

        RpcException already = await Assert.ThrowsAsync<RpcException>(() =>
            service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice)));
        RpcException unknown = await Assert.ThrowsAsync<RpcException>(() =>
            service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = 999 }, ContextFor(Alice)));

        Assert.Equal(StatusCode.FailedPrecondition, already.StatusCode);
        Assert.Equal(StatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task DeleteFriend_EndsTheFriendship_AndTheProfileSaysSo()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        await BefriendAsync(Alice, Bob);
        UserServiceImpl users = new(_store, new UserDataSeeder(Options.Create(new Configuration.ServerOptions())), MasterDb, TestServices.Missions(MasterDb), _time);

        Assert.True((await users.GetUserProfile(new GetUserProfileRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice))).IsFriend);

        await Service().DeleteFriend(new DeleteFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));

        Assert.False((await users.GetUserProfile(new GetUserProfileRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice))).IsFriend);
        Assert.Empty(_store.Friends.FriendsOf(Bob));
    }

    [Fact]
    public async Task Cheer_GivesBothSidesOneStamina_OncePerFriendPerDay()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        DarkUserMemoryDatabase bob = AddPlayer(Bob, "Bob");
        await BefriendAsync(Alice, Bob);
        FriendServiceImpl service = Service();

        await service.CheerFriend(new CheerFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));

        Assert.Equal(11_000, Stamina(alice));
        GetFriendListResponse aliceList = await service.GetFriendList(new GetFriendListRequest(), ContextFor(Alice));
        Assert.True(aliceList.FriendUser[0].CheerSent);
        Assert.Equal(1, aliceList.SendCheerCount);
        GetFriendListResponse bobList = await service.GetFriendList(new GetFriendListRequest(), ContextFor(Bob));
        Assert.True(bobList.FriendUser[0].CheerReceived);
        Assert.False(bobList.FriendUser[0].StaminaReceived);

        await Assert.ThrowsAsync<RpcException>(() =>
            service.CheerFriend(new CheerFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice)));

        await service.ReceiveCheer(new ReceiveCheerRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Bob));

        Assert.Equal(11_000, Stamina(bob));
        bobList = await service.GetFriendList(new GetFriendListRequest(), ContextFor(Bob));
        Assert.False(bobList.FriendUser[0].CheerReceived);
        Assert.True(bobList.FriendUser[0].StaminaReceived);
        Assert.Equal(1, bobList.ReceivedCheerCount);
        await Assert.ThrowsAsync<RpcException>(() =>
            service.ReceiveCheer(new ReceiveCheerRequest { PlayerId = PlayerOf(Alice) }, ContextFor(Bob)));
    }

    [Fact]
    public async Task Cheers_ResetAtTheNextGameDay_AndAnUntakenCheerLapses()
    {
        AddPlayer(Alice, "Alice");
        AddPlayer(Bob, "Bob");
        await BefriendAsync(Alice, Bob);
        FriendServiceImpl service = Service();
        await service.CheerFriend(new CheerFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));

        _time.Advance(TimeSpan.FromHours(20)); // 08:00 UTC the next day

        GetFriendListResponse aliceList = await service.GetFriendList(new GetFriendListRequest(), ContextFor(Alice));
        Assert.False(aliceList.FriendUser[0].CheerSent);
        Assert.Equal(0, aliceList.SendCheerCount);
        Assert.False((await service.GetFriendList(new GetFriendListRequest(), ContextFor(Bob))).FriendUser[0].CheerReceived);
        await service.CheerFriend(new CheerFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));
    }

    [Fact]
    public async Task SendAllAndTakeAll_CheerEveryFriendOnce()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        DarkUserMemoryDatabase bob = AddPlayer(Bob, "Bob");
        AddPlayer(Carol, "Carol");
        await BefriendAsync(Alice, Bob);
        await BefriendAsync(Alice, Carol);
        await BefriendAsync(Bob, Carol);
        FriendServiceImpl service = Service();
        await service.CheerFriend(new CheerFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Alice));

        BulkCheerFriendResponse sent = await service.BulkCheerFriend(new Empty(), ContextFor(Alice));
        Assert.Equal([PlayerOf(Carol)], sent.PlayerId);
        Assert.Equal(12_000, Stamina(alice));

        await service.CheerFriend(new CheerFriendRequest { PlayerId = PlayerOf(Bob) }, ContextFor(Carol));
        BulkReceiveCheerResponse taken = await service.BulkReceiveCheer(new Empty(), ContextFor(Bob));
        Assert.Equal([PlayerOf(Alice), PlayerOf(Carol)], taken.PlayerId.Order());
        Assert.Equal(12_000, Stamina(bob));
        Assert.Empty((await service.BulkReceiveCheer(new Empty(), ContextFor(Bob))).PlayerId);
    }

    [Fact]
    public async Task Recommendations_PutRecentPlayersNearMyLevelFirst_AndSkipFriendsAndRequests()
    {
        long longAgo = NowMs - (long)TimeSpan.FromDays(30).TotalMilliseconds;
        AddPlayer(Alice, "Alice", level: 50);
        AddPlayer(Bob, "Bob", level: 49);                           // friend: left out
        AddPlayer(Carol, "Carol", level: 51);                       // has a request from me: left out
        AddPlayer(2001, "Near", level: 52);                         // recent, 2 levels off
        AddPlayer(2002, "Far", level: 90);                          // recent, 40 levels off
        AddPlayer(2003, "NearButAway", level: 50, lastLoginMs: longAgo); // closest level, but not recent
        AddPlayer(2004, "", level: 50);                             // no name yet (tutorial): left out
        await BefriendAsync(Alice, Bob);
        FriendServiceImpl service = Service();
        await service.SendFriendRequest(new SendFriendRequestRequest { PlayerId = PlayerOf(Carol) }, ContextFor(Alice));

        SearchRecommendedUsersResponse response = await service.SearchRecommendedUsers(new Empty(), ContextFor(Alice));

        Assert.Equal(["Near", "Far", "NearButAway"], response.Users.Select(u => u.UserName));
    }
}
