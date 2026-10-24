using MariesWonderland.Configuration;
using MariesWonderland.Constants;
using MariesWonderland.Data;
using MariesWonderland.Interceptors;
using MariesWonderland.Models.Entities;
using MariesWonderland.Persistence;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Tutorial;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Grpc.Core;
using MariesWonderland.Missions;
using TutorialServiceImpl = MariesWonderland.Services.TutorialService;
using UserServiceImpl = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Persistence;

/// <summary>
/// The SQLite user database: the real save round trip, request changes written through the diff interceptor,
/// users, devices, sessions and shared prizes surviving a restart, idle eviction, and recovery from drift.
/// </summary>
public sealed class UserPersistenceTests : IDisposable
{
    private const long UserId = 42;
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"mw-users-{Guid.NewGuid():N}.db");
    private readonly EfUserPersistence _persistence;
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 24, 12, 0, 0, TimeSpan.Zero));

    public UserPersistenceTests()
    {
        Microsoft.EntityFrameworkCore.DbContextOptions<SqliteUserDbContext> options = SqliteUserDbContext.Options($"Data Source={_path}");
        _persistence = new EfUserPersistence(() => new SqliteUserDbContext(options));
        _persistence.Migrate();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    /// <summary>A server restart: a new store over the same database.</summary>
    private UserDataStore NewStore() => new(_time, _persistence, NullLogger<UserDataStore>.Instance);

    /// <summary>Every row of every table, as JSON, order aside.</summary>
    private static Dictionary<string, List<string>> Rows(DarkUserMemoryDatabase db)
        => UserTables.All.ToDictionary(
            t => t.Name,
            t => ((System.Collections.IEnumerable)t.Property.GetValue(db)!).Cast<object>()
                .Select(r => System.Text.Json.JsonSerializer.Serialize(r, r.GetType())).Order().ToList());

    [Fact]
    public void RealSave_RoundTripsThroughTheDatabase()
    {
        DarkUserMemoryDatabase save = new UserDataSeeder(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } })).LoadFromFiles();
        save.UserId = save.EntityIUser.Single().UserId;

        _persistence.ReplaceUser(save);
        DarkUserMemoryDatabase loaded = _persistence.LoadUser(save.UserId)!;

        Dictionary<string, List<string>> expected = Rows(save);
        Dictionary<string, List<string>> actual = Rows(loaded);
        Assert.All(expected.Keys, table => Assert.True(expected[table].SequenceEqual(actual[table]), $"{table} differs"));
        Assert.True(expected.Values.Sum(r => r.Count) > 10_000);
    }

    [Fact]
    public async Task RequestChanges_AreWrittenAndSurviveARestart()
    {
        UserDataStore store = NewStore();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(UserId);
        userDb.EntityIUserMaterial.AddRange(
            new EntityIUserMaterial { UserId = UserId, MaterialId = 1, Count = 5 },
            new EntityIUserMaterial { UserId = UserId, MaterialId = 2, Count = 7 });
        store.Set(UserId, userDb);

        UserLockInterceptor locks = new(store);
        DiffInterceptor diffs = new(store, NullLogger<DiffInterceptor>.Instance);
        // Add, change and remove rows in a client table and a server-only one with a JSON column.
        await locks.UnaryServerHandler(new SetUserNameRequest(), FakeServerCallContext.For(UserId), (request, context) =>
            diffs.UnaryServerHandler(request, context, (_, _) =>
            {
                DarkUserMemoryDatabase db = store.GetOrCreate(UserId);
                db.EntityIUserMaterial.Single(m => m.MaterialId == 1).Count = 6;
                db.EntityIUserMaterial.RemoveAll(m => m.MaterialId == 2);
                db.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = 3, Count = 1 });
                db.EntitySGachaBannerState.Add(new EntitySGachaBannerState { UserId = UserId, GachaId = 300001, BoxDrewCounts = { [0] = 2, [5] = 1 } });
                return Task.FromResult(new SetUserNameResponse());
            }));

        Assert.True(NewStore().TryGet(UserId, out DarkUserMemoryDatabase reloaded));
        Assert.Equal([(1, 6), (3, 1)], reloaded.EntityIUserMaterial.OrderBy(m => m.MaterialId).Select(m => (m.MaterialId, m.Count)));
        EntitySGachaBannerState banner = Assert.Single(reloaded.EntitySGachaBannerState);
        Assert.Equal(new Dictionary<int, int> { [0] = 2, [5] = 1 }, banner.BoxDrewCounts);
    }

    [Fact]
    public async Task StarterDeck_IsWrittenAndSurvivesARestart()
    {
        UserDataStore store = NewStore();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(UserId);
        userDb.EntityIUserCostume.Add(new EntityIUserCostume { UserId = UserId, UserCostumeUuid = "starter-costume", CostumeId = GameConstants.StartingDeckCostumeId });
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon { UserId = UserId, UserWeaponUuid = "starter-weapon", WeaponId = GameConstants.StartingDeckWeaponId });
        store.Set(UserId, userDb);

        TutorialServiceImpl service = new(store, new DarkMasterMemoryDatabase(), _time);
        UserLockInterceptor locks = new(store);
        DiffInterceptor diffs = new(store, NullLogger<DiffInterceptor>.Instance);
        SetTutorialProgressRequest tutorial = new() { TutorialType = (int)TutorialType.MENU_FIRST, ProgressPhase = 10 };
        await locks.UnaryServerHandler(tutorial, FakeServerCallContext.For(UserId), (request, context) =>
            diffs.UnaryServerHandler(request, context, (r, c) => service.SetTutorialProgress(r, c)));

        Assert.True(NewStore().TryGet(UserId, out DarkUserMemoryDatabase reloaded));
        EntityIUserDeck deck = Assert.Single(reloaded.EntityIUserDeck);
        Assert.Equal("", deck.UserDeckCharacterUuid02);
        Assert.Single(reloaded.EntityIUserDeckCharacter);
        Assert.Contains(reloaded.EntityIUserTutorialProgress, t => t.TutorialType == TutorialType.MENU_FIRST);
    }

    [Fact]
    public void BattleDetails_CanBeReadForOneQuestSinceItsStart()
    {
        UserDataStore store = NewStore();
        DateTime start = _time.GetUtcNow().UtcDateTime;
        store.RecordBattle(new EntitySBattleDetail { UserId = UserId, QuestId = 5, RecordedAt = start.AddMinutes(-10), MaxDamage = 1 });
        store.RecordBattle(new EntitySBattleDetail { UserId = UserId, QuestId = 5, RecordedAt = start.AddMinutes(1), MaxDamage = 2 });
        store.RecordBattle(new EntitySBattleDetail { UserId = UserId, QuestId = 8, RecordedAt = start.AddMinutes(2), MaxDamage = 3 });

        EntitySBattleDetail detail = Assert.Single(store.BattleDetails(UserId, 5, start));

        Assert.Equal(2, detail.MaxDamage);
    }

    [Fact]
    public void UsersDevicesAndSessions_SurviveARestart()
    {
        UserDataStore store = NewStore();
        (long userId, bool isNew) = store.RegisterOrGetUser("device-1", new DarkMasterMemoryDatabase());
        UserSession session = store.CreateSession(userId, TimeSpan.FromHours(24));
        Assert.True(isNew);

        UserDataStore restarted = NewStore();
        Assert.Equal((userId, false), restarted.RegisterOrGetUser("device-1", new DarkMasterMemoryDatabase()));
        Assert.True(restarted.TryResolveSession(session.SessionKey, out long sessionUserId));
        Assert.Equal(userId, sessionUserId);
        Assert.True(restarted.TryGet(userId, out DarkUserMemoryDatabase userDb));
        Assert.Equal(userId, Assert.Single(userDb.EntityIUser).UserId);

        long playerId = userDb.EntityIUser.Single().PlayerId;
        Assert.True(NewStore().TryGetByPlayerId(playerId, out DarkUserMemoryDatabase byPlayer));
        Assert.Equal(userId, byPlayer.UserId);

        _time.Advance(TimeSpan.FromHours(25));
        Assert.False(NewStore().TryResolveSession(session.SessionKey, out _));
    }

    [Fact]
    public void SharedPrizeClaims_SurviveARestart()
    {
        UserDataStore store = NewStore();
        Assert.True(store.TryClaimSharedPrize(200001, 0, stock: 2));
        Assert.True(store.TryClaimSharedPrize(200001, 0, stock: 2));

        UserDataStore restarted = NewStore();
        Assert.Equal(2, restarted.SharedPrizeClaimed(200001, 0));
        Assert.False(restarted.TryClaimSharedPrize(200001, 0, stock: 2));
    }

    [Fact]
    public void EvictIdle_DropsIdleUsersButNotBusyOnes_AndTheyLoadAgain()
    {
        UserDataStore store = NewStore();
        (long idle, _) = store.RegisterOrGetUser("device-idle", new DarkMasterMemoryDatabase());
        (long busy, _) = store.RegisterOrGetUser("device-busy", new DarkMasterMemoryDatabase());
        _time.Advance(TimeSpan.FromMinutes(31));

        using (store.Locks.TryAcquire(busy, TimeSpan.Zero))
        {
            Assert.Equal(1, store.EvictIdle(TimeSpan.FromMinutes(30)));
        }

        Assert.False(store.All.ContainsKey(idle));
        Assert.True(store.All.ContainsKey(busy));
        Assert.True(store.TryGet(idle, out DarkUserMemoryDatabase reloaded));
        Assert.Equal(idle, reloaded.EntityIUser.Single().UserId);
    }

    [Fact]
    public void BattleLog_IsWrittenStraightToTheDatabase_NotKeptOnTheUser()
    {
        UserDataStore store = NewStore();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(UserId);
        store.RecordBattle(new EntitySBattleDetail { UserId = UserId, QuestId = 7, RecordedAt = _time.GetUtcNow().UtcDateTime, MaxDamage = 1234 });
        // Same key again: logged and skipped, not thrown.
        store.RecordBattle(new EntitySBattleDetail { UserId = UserId, QuestId = 7, RecordedAt = _time.GetUtcNow().UtcDateTime });

        EntitySBattleDetail saved = Assert.Single(NewStore().BattleDetails(UserId));
        Assert.Equal(1234, saved.MaxDamage);
        Assert.DoesNotContain(UserTables.All, t => t.RowType == typeof(EntitySBattleDetail));

        // Replacing the user (a save point load) leaves the log alone.
        store.Set(UserId, userDb);
        Assert.Single(store.BattleDetails(UserId));
    }

    [Fact]
    public void SavePoints_SaveTestLoad_PutsTheUserBack()
    {
        string directory = Path.ChangeExtension(_path, null) + "-saves";
        try
        {
            UserDataStore store = NewStore();
            (long userId, _) = store.RegisterOrGetUser("device-save", new DarkMasterMemoryDatabase());
            UserSession session = store.CreateSession(userId, TimeSpan.FromHours(24));
            SavePoints savePoints = new(store, directory);

            Assert.Equal(userId, savePoints.Save("before-test", null));

            // The test: changes made through a request.
            DarkUserMemoryDatabase userDb = store.GetOrCreate(userId);
            Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(userDb);
            userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = userId, MaterialId = 1, Count = 99 });
            userDb.EntityIUserStatus.Single().Level = 50;
            store.SaveChanges(userId, before, userDb);

            Assert.Equal(userId, savePoints.Load("before-test"));

            // Back as saved, in memory and after a restart, still signed in.
            foreach (UserDataStore s in new[] { store, NewStore() })
            {
                Assert.True(s.TryGet(userId, out DarkUserMemoryDatabase restored));
                Assert.Empty(restored.EntityIUserMaterial);
                Assert.Equal(1, restored.EntityIUserStatus.Single().Level);
                Assert.True(s.TryResolveSession(session.SessionKey, out long sessionUser));
                Assert.Equal(userId, sessionUser);
            }
            Assert.Equal(["before-test"], savePoints.List().Select(p => p.Name));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Fact]
    public void SavePoints_RejectBadNamesMissingSavesAndAmbiguousUsers()
    {
        string directory = Path.ChangeExtension(_path, null) + "-saves";
        UserDataStore store = NewStore();
        SavePoints savePoints = new(store, directory);

        Assert.Throws<ArgumentException>(() => savePoints.Save(null, null));
        store.RegisterOrGetUser("device-a", new DarkMasterMemoryDatabase());
        store.RegisterOrGetUser("device-b", new DarkMasterMemoryDatabase());
        Assert.Contains("?user=", Assert.Throws<ArgumentException>(() => savePoints.Save(null, null)).Message);
        Assert.Throws<ArgumentException>(() => savePoints.Save("../escape", null));
        Assert.Throws<ArgumentException>(() => savePoints.Load("never-saved"));
        Assert.Empty(savePoints.List());
    }

    [Fact]
    public void InMemoryStore_NeverEvicts()
    {
        UserDataStore store = new(_time);
        store.GetOrCreate(UserId);
        _time.Advance(TimeSpan.FromDays(1));

        Assert.Equal(0, store.EvictIdle(TimeSpan.FromMinutes(1)));
        Assert.True(store.All.ContainsKey(UserId));
    }

    [Fact]
    public void SaveChanges_WhenTheDatabaseDrifted_WritesTheWholeUser()
    {
        UserDataStore store = NewStore();
        DarkUserMemoryDatabase userDb = store.GetOrCreate(UserId);
        // In memory only: the database never got this row, so updating it fails and the store rewrites the user.
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = UserId, MaterialId = 1, Count = 1 });
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(userDb);
        userDb.EntityIUserMaterial[0].Count = 9;

        store.SaveChanges(UserId, before, userDb);

        EntityIUserMaterial saved = Assert.Single(_persistence.LoadUser(UserId)!.EntityIUserMaterial);
        Assert.Equal(9, saved.Count);
    }

    [Fact]
    public void Load_ImportsTheJsonSnapshotIntoTheDatabase()
    {
        UserDataStore memoryOnly = new(_time);
        (long userId, _) = memoryOnly.RegisterOrGetUser("device-json", new DarkMasterMemoryDatabase());
        string json = Path.ChangeExtension(_path, ".json");
        try
        {
            // The whole-server file the server kept before it had a user database.
            File.WriteAllText(json, System.Text.Json.JsonSerializer.Serialize(new
            {
                Users = new Dictionary<long, DarkUserMemoryDatabase> { [userId] = memoryOnly.All[userId] },
                UuidToUserId = new Dictionary<string, long> { ["device-json"] = userId },
                Sessions = Array.Empty<UserSession>(),
                SharedPrizeClaims = new Dictionary<string, int>(),
            }));
            Assert.Equal(1, NewStore().Load(json));
        }
        finally
        {
            File.Delete(json);
        }

        UserDataStore restarted = NewStore();
        Assert.Equal((userId, false), restarted.RegisterOrGetUser("device-json", new DarkMasterMemoryDatabase()));
        Assert.True(restarted.TryGet(userId, out _));
    }

    private UserServiceImpl TransferService(UserDataStore store, string userDataPath = "Data/UserData")
    {
        DarkMasterMemoryDatabase masterDb = new();
        UserDataSeeder seeder = new(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = userDataPath } }));
        return new UserServiceImpl(store, seeder, masterDb, new MissionTracker(masterDb, new MissionCatalog(masterDb, MissionFilterTable.Empty)), _time);
    }

    [Fact]
    public async Task TransferUser_LoadsTheRealSave_SignsIntoIt_AndItSurvivesARestart()
    {
        DarkUserMemoryDatabase save = new UserDataSeeder(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } })).LoadFromFiles();
        long saveUserId = save.EntityIUser.Single().UserId;

        UserDataStore store = NewStore();
        TransferUserResponse transfer = await TransferService(store).TransferUser(new TransferUserRequest { Uuid = "transfer-device" }, FakeServerCallContext.Anonymous());
        AuthUserResponse auth = await TransferService(store).Auth(new AuthUserRequest { Uuid = "transfer-device", Signature = transfer.Signature }, FakeServerCallContext.Anonymous());

        Assert.Equal(saveUserId, transfer.UserId);
        Assert.Equal(saveUserId, auth.UserId);
        Assert.True(NewStore().TryGet(saveUserId, out DarkUserMemoryDatabase reloaded));
        Dictionary<string, List<string>> expected = Rows(save);
        Dictionary<string, List<string>> actual = Rows(reloaded);
        // Signing in records the device and counts the day's login.
        Assert.All(expected.Keys.Where(t => t is not ("SUserDevice" or "IUserLogin")), table => Assert.True(expected[table].SequenceEqual(actual[table]), $"{table} differs"));
        Assert.Equal("transfer-device", Assert.Single(reloaded.EntitySUserDevice).Uuid);
        Assert.Equal(Assert.Single(save.EntityIUserLogin).TotalLoginCount + 1, Assert.Single(reloaded.EntityIUserLogin).TotalLoginCount);
    }

    [Fact]
    public async Task TransferUser_Again_ResetsTheAccountToTheSave()
    {
        UserDataStore store = NewStore();
        TransferUserResponse first = await TransferService(store).TransferUser(new TransferUserRequest { Uuid = "phone-1" }, FakeServerCallContext.Anonymous());
        DarkUserMemoryDatabase played = store.GetOrCreate(first.UserId);
        int savedLevel = played.EntityIUserStatus.Single().Level;
        played.EntityIUserStatus.Single().Level = 1;
        store.Set(first.UserId, played);

        TransferUserResponse second = await TransferService(store).TransferUser(new TransferUserRequest { Uuid = "phone-2" }, FakeServerCallContext.Anonymous());

        Assert.Equal(first.UserId, second.UserId);
        Assert.Equal(savedLevel, store.GetOrCreate(first.UserId).EntityIUserStatus.Single().Level);
        Assert.True(NewStore().TryGet(first.UserId, out DarkUserMemoryDatabase reloaded));
        Assert.Equal(savedLevel, reloaded.EntityIUserStatus.Single().Level);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Data/NoSuchSave")]
    public async Task TransferUser_WithoutASave_FailsAndCreatesNothing(string userDataPath)
    {
        UserDataStore store = NewStore();

        RpcException error = await Assert.ThrowsAsync<RpcException>(() => TransferService(store, userDataPath).TransferUser(new TransferUserRequest { Uuid = "transfer-device" }, FakeServerCallContext.Anonymous()));

        Assert.Equal(StatusCode.FailedPrecondition, error.StatusCode);
        Assert.Empty(_persistence.UserIds());
        Assert.Null(_persistence.FindUserIdByDevice("transfer-device"));
    }
}
