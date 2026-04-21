using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Explore;
using MariesWonderland.Tests.Infrastructure;
using ExploreService = MariesWonderland.Services.ExploreService;

namespace MariesWonderland.Tests.Services;

public class ExploreServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1L;
    private const int ExploreId = 1;         // Normal explore, requires quest 31 cleared
    private const int UnlockQuestId = 31;
    private const int TicketItemId = 2001;   // ConsumableItemIdForExploreTicket

    private ExploreService CreateService(UserDataStore store) => new(store, MasterDb, GameConfig);

    private static void AddQuestCleared(DarkUserMemoryDatabase userDb, int questId = UnlockQuestId)
        => userDb.EntityIUserQuest.Add(new EntityIUserQuest
        {
            UserId = UserId,
            QuestId = questId,
            QuestStateType = (int)QuestStateType.CLEARED
        });

    private static EntityIUserExplore AddUserExplore(DarkUserMemoryDatabase userDb,
        int playingExploreId = 0, bool isUsingTicket = false)
    {
        var entity = new EntityIUserExplore
        {
            UserId = UserId,
            PlayingExploreId = playingExploreId,
            IsUseExploreTicket = isUsingTicket
        };
        userDb.EntityIUserExplore.Add(entity);
        return entity;
    }

    private static EntityIUserStatus AddUserStatus(DarkUserMemoryDatabase userDb,
        int staminaMilliValue = 5000, int level = 0, int exp = 0)
    {
        var entity = new EntityIUserStatus { UserId = UserId, StaminaMilliValue = staminaMilliValue, Level = level, Exp = exp };
        userDb.EntityIUserStatus.Add(entity);
        return entity;
    }

    #region StartExplore

    [Fact]
    public async Task StartExplore_FreePlay_SetsPlayingStateWithoutDeductingItem()
    {
        var userDb = CreateUserDb();
        AddQuestCleared(userDb);
        var userExplore = AddUserExplore(userDb);
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId, ConsumableItemId = TicketItemId, Count = 3
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.StartExplore(
            new StartExploreRequest { ExploreId = ExploreId, UseConsumableItemId = 0 },
            ContextFor(UserId));

        Assert.Equal(ExploreId, userExplore.PlayingExploreId);
        Assert.False(userExplore.IsUseExploreTicket);
        Assert.True(userExplore.LatestPlayDatetime > 0);
        Assert.Equal(3, userDb.EntityIUserConsumableItem[0].Count); // not deducted
    }

    [Fact]
    public async Task StartExplore_WithTicket_DeductsTicketAndSetsFlag()
    {
        var userDb = CreateUserDb();
        AddQuestCleared(userDb);
        var userExplore = AddUserExplore(userDb);
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId, ConsumableItemId = TicketItemId, Count = 3
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.StartExplore(
            new StartExploreRequest { ExploreId = ExploreId, UseConsumableItemId = TicketItemId },
            ContextFor(UserId));

        Assert.Equal(ExploreId, userExplore.PlayingExploreId);
        Assert.True(userExplore.IsUseExploreTicket);
        Assert.Equal(2, userDb.EntityIUserConsumableItem[0].Count); // 3 - 1 = 2
    }

    [Fact]
    public async Task StartExplore_InvalidExploreId_ThrowsInvalidArgument()
    {
        var store = CreateStore(UserId, CreateUserDb(), MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.StartExplore(
                new StartExploreRequest { ExploreId = 9999 },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task StartExplore_UnlockConditionNotMet_ThrowsFailedPrecondition()
    {
        var userDb = CreateUserDb();
        AddUserExplore(userDb); // quest 31 NOT cleared
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.StartExplore(
                new StartExploreRequest { ExploreId = ExploreId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    [Fact]
    public async Task StartExplore_TicketNotFound_ThrowsFailedPrecondition()
    {
        var userDb = CreateUserDb();
        AddQuestCleared(userDb);
        AddUserExplore(userDb); // no consumable item record
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.StartExplore(
                new StartExploreRequest { ExploreId = ExploreId, UseConsumableItemId = TicketItemId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    [Fact]
    public async Task StartExplore_InsufficientTickets_ThrowsFailedPrecondition()
    {
        var userDb = CreateUserDb();
        AddQuestCleared(userDb);
        AddUserExplore(userDb);
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId, ConsumableItemId = TicketItemId, Count = 0
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.StartExplore(
                new StartExploreRequest { ExploreId = ExploreId, UseConsumableItemId = TicketItemId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    #endregion

    #region RetireExplore

    [Fact]
    public async Task RetireExplore_WithActiveExplore_ClearsState()
    {
        var userDb = CreateUserDb();
        var userExplore = AddUserExplore(userDb, playingExploreId: ExploreId, isUsingTicket: true);
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.RetireExplore(
            new RetireExploreRequest { ExploreId = ExploreId },
            ContextFor(UserId));

        Assert.Equal(0, userExplore.PlayingExploreId);
        Assert.False(userExplore.IsUseExploreTicket);
    }

    [Fact]
    public async Task RetireExplore_NoMatchingActiveExplore_ThrowsFailedPrecondition()
    {
        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: 0);
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.RetireExplore(
                new RetireExploreRequest { ExploreId = ExploreId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    [Fact]
    public async Task RetireExplore_UserExploreNotFound_ThrowsFailedPrecondition()
    {
        var store = CreateStore(UserId, CreateUserDb(), MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.RetireExplore(
                new RetireExploreRequest { ExploreId = ExploreId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    #endregion

    #region FinishExplore

    [Fact]
    public async Task FinishExplore_NewHighScore_GrantsRewardsAndClearsState()
    {
        // Score 50000 hits the 50000 threshold → ExploreGradeId 102 → AssetGradeIconId 17000
        const int score = 50000;
        const int expectedGradeIconId = 17000;

        var userDb = CreateUserDb();
        var userExplore = AddUserExplore(userDb, playingExploreId: ExploreId, isUsingTicket: true);
        var status = AddUserStatus(userDb, staminaMilliValue: 5000, level: 229);
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.FinishExplore(
            new FinishExploreRequest { ExploreId = ExploreId, Score = score },
            ContextFor(UserId));

        // Score record created
        Assert.Single(userDb.EntityIUserExploreScore);
        Assert.Equal(score, userDb.EntityIUserExploreScore[0].MaxScore);

        // Active state cleared
        Assert.Equal(0, userExplore.PlayingExploreId);
        Assert.False(userExplore.IsUseExploreTicket);

        // Stamina recovered (50 base × RewardLotteryCount 1 = 50 stamina = 50000 milli)
        Assert.Equal(55000, status.StaminaMilliValue);

        // EXP granted: 0.5% of level 229 requirement (884,516) × 1 = 4,422
        Assert.Equal(4422, status.Exp);

        // Material granted: 1 draw × 8 per draw (grade 102) = 8 of material 100001
        Assert.Single(userDb.EntityIUserMaterial);
        Assert.Equal(8, userDb.EntityIUserMaterial[0].Count);

        // Response populated
        Assert.Equal(50, response.AcquireStaminaCount);
        Assert.Equal(expectedGradeIconId, response.AssetGradeIconId);
        Assert.Single(response.ExploreReward);
        Assert.Equal(8, response.ExploreReward[0].Count);
    }

    [Fact]
    public async Task FinishExplore_ImprovedScore_UpdatesMaxScore()
    {
        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: ExploreId);
        AddUserStatus(userDb);
        userDb.EntityIUserExploreScore.Add(new EntityIUserExploreScore
        {
            UserId = UserId, ExploreId = ExploreId, MaxScore = 50000
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.FinishExplore(
            new FinishExploreRequest { ExploreId = ExploreId, Score = 80000 },
            ContextFor(UserId));

        Assert.Equal(80000, userDb.EntityIUserExploreScore[0].MaxScore);
    }

    [Fact]
    public async Task FinishExplore_LowerScore_DoesNotUpdateMaxScore()
    {
        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: ExploreId);
        AddUserStatus(userDb);
        userDb.EntityIUserExploreScore.Add(new EntityIUserExploreScore
        {
            UserId = UserId, ExploreId = ExploreId, MaxScore = 80000
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.FinishExplore(
            new FinishExploreRequest { ExploreId = ExploreId, Score = 50000 },
            ContextFor(UserId));

        Assert.Equal(80000, userDb.EntityIUserExploreScore[0].MaxScore);
    }

    [Fact]
    public async Task FinishExplore_InvalidExploreId_ThrowsInvalidArgument()
    {
        var store = CreateStore(UserId, CreateUserDb(), MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.FinishExplore(
                new FinishExploreRequest { ExploreId = 9999 },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }

    [Fact]
    public async Task FinishExplore_NoMatchingActiveExplore_ThrowsFailedPrecondition()
    {
        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: 0); // nothing active
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.FinishExplore(
                new FinishExploreRequest { ExploreId = ExploreId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    [Fact]
    public async Task FinishExplore_UserExploreNotFound_ThrowsFailedPrecondition()
    {
        var store = CreateStore(UserId, CreateUserDb(), MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.FinishExplore(
                new FinishExploreRequest { ExploreId = ExploreId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    [Fact]
    public async Task FinishExplore_UserStatusNotFound_ThrowsFailedPrecondition()
    {
        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: ExploreId); // no status record
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.FinishExplore(
                new FinishExploreRequest { ExploreId = ExploreId },
                ContextFor(UserId)));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
    }

    [Fact]
    public async Task FinishExplore_ExpGrantCrossesThreshold_AdvancesLevel()
    {
        // Level 229 → 230 threshold is 87,191,772. Start 100 EXP below so the grant (4,422) pushes us over.
        const int level230Threshold = 87_191_772;
        const int startExp = level230Threshold - 100;

        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: ExploreId);
        var status = AddUserStatus(userDb, staminaMilliValue: 0, level: 229, exp: startExp);
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.FinishExplore(
            new FinishExploreRequest { ExploreId = ExploreId, Score = 0 },
            ContextFor(UserId));

        Assert.Equal(230, status.Level);
        Assert.True(status.Exp >= level230Threshold);
    }

    [Fact]
    public async Task FinishExplore_WorstGrade_GrantsNoMaterials()
    {
        // Score 0 → grade 106 (worst) → stackPerDraw = 0 → no ExploreReward entries
        var userDb = CreateUserDb();
        AddUserExplore(userDb, playingExploreId: ExploreId);
        AddUserStatus(userDb, level: 229);
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.FinishExplore(
            new FinishExploreRequest { ExploreId = ExploreId, Score = 0 },
            ContextFor(UserId));

        Assert.Empty(userDb.EntityIUserMaterial);
        Assert.Empty(response.ExploreReward);
    }

    #endregion
}
