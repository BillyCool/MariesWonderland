using Google.Protobuf;
using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;
using MariesWonderland.Tests.Infrastructure;
using BattleServiceImpl = MariesWonderland.Services.BattleService;

namespace MariesWonderland.Tests.Services;

public class BattleServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task StartWave_WithoutExistingUser_CreatesUserDatabaseAndReturnsEmptyResponse()
    {
        UserDataStore store = CreateStore();
        BattleServiceImpl service = CreateService(store);

        StartWaveResponse response = await service.StartWave(
            new StartWaveRequest(),
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.True(store.TryGet(UserId, out DarkUserMemoryDatabase? userDb));
        Assert.NotNull(userDb);
        Assert.Equal(UserId, userDb.UserId);
        Assert.Empty(store.BattleDetails(UserId));
    }

    [Fact]
    public async Task FinishWave_WithFasterClear_RecordsBattleAndUpdatesShortestClearFrames()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = 101,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ShortestClearFrames = 600
        });
        UserDataStore store = CreateStore(userDb);
        BattleServiceImpl service = CreateService(store);
        FinishWaveRequest request = new()
        {
            BattleBinary = ByteString.CopyFrom([1, 2, 3]),
            ElapsedFrameCount = 480,
            BattleDetail = new BattleDetail
            {
                MaxDamage = 321,
                PlayerCostumeActiveSkillUsedCount = 2,
                PlayerWeaponActiveSkillUsedCount = 3,
                PlayerCompanionSkillUsedCount = 4,
                CriticalCount = 5,
                ComboCount = 6,
                ComboMaxDamage = 654,
                TotalRecoverPoint = 987,
                CostumeBattleInfo =
                {
                    new CostumeBattleInfo
                    {
                        DeckCharacterNumber = 1,
                        TotalDamage = 111,
                        IsAlive = true,
                        HitCount = 7,
                        MaxHp = 1000,
                        RemainingHp = 900,
                        UserDeckNumber = 1
                    },
                    new CostumeBattleInfo
                    {
                        DeckCharacterNumber = 2,
                        TotalDamage = 222,
                        IsAlive = false,
                        HitCount = 8,
                        MaxHp = 800,
                        RemainingHp = 0,
                        UserDeckNumber = 1
                    }
                },
            },
            UserPartyResultInfoList =
            {
                new UserPartyResultInfo
                {
                    UserId = UserId,
                    DeckType = 1,
                    UserDeckNumber = 3,
                    CharacterDeathCount = 1
                }
            },
            NpcPartyResultInfoList =
            {
                new NpcPartyResultInfo
                {
                    NpcId = 99,
                    DeckType = 2,
                    BattleNpcDeckNumber = 4,
                    CharacterReviveCount = 1
                }
            }
        };
        DateTime before = DateTime.UtcNow;

        FinishWaveResponse response = await service.FinishWave(request, FakeServerCallContext.For(UserId));

        DateTime after = DateTime.UtcNow;
        EntityIUserQuest activeQuest = Assert.Single(userDb.EntityIUserQuest);
        EntitySBattleDetail battleRecord = Assert.Single(store.BattleDetails(UserId));
        Assert.NotNull(response);
        Assert.Equal(480, activeQuest.ShortestClearFrames);
        Assert.Equal(101, battleRecord.QuestId);
        Assert.Equal(UserId, battleRecord.UserId);
        Assert.Equal(480, battleRecord.ElapsedFrameCount);
        Assert.Equal("AQID", battleRecord.BattleBinary);
        Assert.InRange(battleRecord.RecordedAt, before, after);
        Assert.Equal(321, battleRecord.MaxDamage);
        Assert.Equal(2, battleRecord.PlayerCostumeActiveSkillUsedCount);
        Assert.Equal(3, battleRecord.PlayerWeaponActiveSkillUsedCount);
        Assert.Equal(4, battleRecord.PlayerCompanionSkillUsedCount);
        Assert.Equal(5, battleRecord.CriticalCount);
        Assert.Equal(6, battleRecord.ComboCount);
        Assert.Equal(654, battleRecord.ComboMaxDamage);
        Assert.Equal(987, battleRecord.TotalRecoverPoint);
        Assert.Equal(2, battleRecord.CostumeBattleInfoList.Count);
        Assert.False(battleRecord.CostumeBattleInfoList[1].IsAlive);
        Assert.Equal(1, battleRecord.CharacterDeathCount);
        Assert.Single(battleRecord.UserPartyResultInfoList);
        Assert.Equal(3, battleRecord.UserPartyResultInfoList[0].UserDeckNumber);
        Assert.Single(battleRecord.NpcPartyResultInfoList);
        Assert.Equal(99, battleRecord.NpcPartyResultInfoList[0].NpcId);
    }

    [Fact]
    public async Task FinishWave_WithZeroShortestClearFramesAndNoBattleDetail_UsesDefaultValues()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = 202,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ShortestClearFrames = 0
        });
        UserDataStore store = CreateStore(userDb);
        BattleServiceImpl service = CreateService(store);

        await service.FinishWave(
            new FinishWaveRequest
            {
                ElapsedFrameCount = 300
            },
            FakeServerCallContext.For(UserId));

        EntityIUserQuest activeQuest = Assert.Single(userDb.EntityIUserQuest);
        EntitySBattleDetail battleRecord = Assert.Single(store.BattleDetails(UserId));
        Assert.Equal(300, activeQuest.ShortestClearFrames);
        Assert.Equal(202, battleRecord.QuestId);
        Assert.Equal(string.Empty, battleRecord.BattleBinary);
        Assert.Equal(300, battleRecord.ElapsedFrameCount);
        Assert.Equal(0, battleRecord.MaxDamage);
        Assert.Equal(0, battleRecord.PlayerCostumeActiveSkillUsedCount);
        Assert.Equal(0, battleRecord.PlayerWeaponActiveSkillUsedCount);
        Assert.Equal(0, battleRecord.PlayerCompanionSkillUsedCount);
        Assert.Equal(0, battleRecord.CriticalCount);
        Assert.Equal(0, battleRecord.ComboCount);
        Assert.Equal(0, battleRecord.ComboMaxDamage);
        Assert.Equal(0, battleRecord.TotalRecoverPoint);
        Assert.Empty(battleRecord.CostumeBattleInfoList);
        Assert.Empty(battleRecord.UserPartyResultInfoList);
        Assert.Empty(battleRecord.NpcPartyResultInfoList);
    }

    [Fact]
    public async Task FinishWave_WithoutActiveQuest_RecordsBattleAgainstQuestZero()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = 303,
            QuestStateType = (int)QuestStateType.CLEARED,
            ShortestClearFrames = 111
        });
        UserDataStore store = CreateStore(userDb);
        BattleServiceImpl service = CreateService(store);

        await service.FinishWave(
            new FinishWaveRequest
            {
                BattleBinary = ByteString.CopyFromUtf8("battle"),
                ElapsedFrameCount = 200
            },
            FakeServerCallContext.For(UserId));

        EntityIUserQuest clearedQuest = Assert.Single(userDb.EntityIUserQuest);
        EntitySBattleDetail battleRecord = Assert.Single(store.BattleDetails(UserId));
        Assert.Equal(111, clearedQuest.ShortestClearFrames);
        Assert.Equal(0, battleRecord.QuestId);
        Assert.Equal("YmF0dGxl", battleRecord.BattleBinary);
    }

    [Fact]
    public async Task FinishWave_WithSlowerClear_DoesNotOverwriteShortestClearFrames()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = 404,
            QuestStateType = (int)QuestStateType.ACTIVE,
            ShortestClearFrames = 150
        });
        UserDataStore store = CreateStore(userDb);
        BattleServiceImpl service = CreateService(store);

        await service.FinishWave(
            new FinishWaveRequest
            {
                ElapsedFrameCount = 200
            },
            FakeServerCallContext.For(UserId));

        Assert.Equal(150, Assert.Single(userDb.EntityIUserQuest).ShortestClearFrames);
        Assert.Single(store.BattleDetails(UserId));
    }

    private static BattleServiceImpl CreateService(UserDataStore store)
    {
        DarkMasterMemoryDatabase masterDb = new();
        return new(store, masterDb, new MissionTracker(masterDb, new MissionCatalog(masterDb, MissionFilterTable.Empty)), TimeProvider.System);
    }

    private static UserDataStore CreateStore(DarkUserMemoryDatabase? userDb = null)
    {
        UserDataStore store = new(TimeProvider.System);
        if (userDb != null)
            store.Set(UserId, userDb);

        return store;
    }
}
