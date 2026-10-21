using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using QuestService = MariesWonderland.Services.QuestService;

namespace MariesWonderland.Tests.Quests;

/// <summary>
/// Auto-orbit (auto-repeat): each run grants its own drops; the run that ends the loop and <c>FinishAutoOrbit</c>
/// list every drop collected across the loop, without granting them again.
/// </summary>
public class AutoOrbitTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private (QuestService Service, DarkUserMemoryDatabase UserDb, long UserId) CreateService()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("auto-orbit-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        QuestCampaignHelper campaigns = new(MasterDb);
        MissionTracker missions = new(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty));
        QuestEngine engine = new(MasterDb, GameConfig, campaigns, new ImportantItemEffectHelper(MasterDb), missions, Options.Create(new DevModeOptions { FreeStamina = true }));
        return (new QuestService(store, MasterDb, engine, campaigns, missions, TimeProvider.System), userDb, userId);
    }

    /// <summary>A playable main quest whose drops are all stackable items, so each run's drop list is the same.</summary>
    private EntityMQuest FindMainQuestWithDrops()
    {
        HashSet<int> mainQuestIds = [.. MasterDb.EntityMMainQuestSequence.Select(s => s.QuestId)];
        HashSet<int> stackableOnlyGroups = [.. MasterDb.EntityMQuestPickupRewardGroup
            .GroupBy(p => p.QuestPickupRewardGroupId)
            .Where(g => g.All(p => MasterDb.EntityMBattleDropReward.Any(d => d.BattleDropRewardId == p.BattleDropRewardId
                && d.PossessionType is Models.Type.PossessionType.MATERIAL or Models.Type.PossessionType.CONSUMABLE_ITEM)))
            .Select(g => g.Key)];
        return MasterDb.EntityMQuest
            .Where(q => mainQuestIds.Contains(q.QuestId) && QuestEngine.IsPlayable(q) && stackableOnlyGroups.Contains(q.QuestPickupRewardGroupId))
            .OrderBy(q => q.QuestId)
            .First();
    }

    private static async Task<FinishMainQuestResponse> RunOnce(QuestService service, long userId, int questId, int maxCount)
    {
        await service.StartMainQuest(new StartMainQuestRequest { QuestId = questId, IsMainFlow = false, UserDeckNumber = 1, MaxAutoOrbitCount = maxCount }, ContextFor(userId));
        return await service.FinishMainQuest(new FinishMainQuestRequest { QuestId = questId, IsMainFlow = false, IsAutoOrbit = true }, ContextFor(userId));
    }

    private static List<string> Keys(IEnumerable<QuestReward> rewards)
        => [.. rewards.Select(r => $"{r.PossessionType}:{r.PossessionId}:{r.Count}")];

    [Fact]
    public async Task Loop_ListsEveryRunsDropsAtTheEnd_AndOnFinishAutoOrbit()
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMQuest quest = FindMainQuestWithDrops();

        FinishMainQuestResponse first = await RunOnce(service, userId, quest.QuestId, maxCount: 2);
        Assert.NotEmpty(first.DropReward);
        Assert.Empty(first.AutoOrbitReward.DropReward);

        FinishMainQuestResponse last = await RunOnce(service, userId, quest.QuestId, maxCount: 2);
        List<string> expected = [.. Keys(first.DropReward), .. Keys(last.DropReward)];
        Assert.Equal(expected, Keys(last.AutoOrbitReward.DropReward));

        FinishAutoOrbitResponse finish = await service.FinishAutoOrbit(new Empty(), ContextFor(userId));
        Assert.Equal(expected, Keys(finish.AutoOrbitReward.DropReward));
        Assert.Empty(userDb.EntitySQuestAutoOrbit);
        Assert.Empty(userDb.EntityIUserQuestAutoOrbit);

        FinishAutoOrbitResponse again = await service.FinishAutoOrbit(new Empty(), ContextFor(userId));
        Assert.Empty(again.AutoOrbitReward.DropReward);
    }

    [Fact]
    public async Task LoopSize_IsCappedAtTen()
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMQuest quest = FindMainQuestWithDrops();

        await service.StartMainQuest(new StartMainQuestRequest { QuestId = quest.QuestId, IsMainFlow = false, UserDeckNumber = 1, MaxAutoOrbitCount = 50 }, ContextFor(userId));

        Assert.Equal(10, userDb.EntityIUserQuestAutoOrbit.Single().MaxAutoOrbitCount);
    }

    [Fact]
    public async Task NewLoop_StartsWithNoCollectedDrops()
    {
        (QuestService service, DarkUserMemoryDatabase userDb, long userId) = CreateService();
        EntityMQuest quest = FindMainQuestWithDrops();

        await RunOnce(service, userId, quest.QuestId, maxCount: 3);
        Assert.NotEmpty(userDb.EntitySQuestAutoOrbit.Single().Drops);

        // A different loop size is a new loop.
        FinishMainQuestResponse run = await RunOnce(service, userId, quest.QuestId, maxCount: 1);
        Assert.Equal(Keys(run.DropReward), Keys(run.AutoOrbitReward.DropReward));
    }
}
