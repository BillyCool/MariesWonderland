using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using MariesWonderland.Helpers;
using MariesWonderland.Proto.Mission;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Quests;
using MissionServiceImpl = MariesWonderland.Services.MissionService;
using QuestServiceImpl = MariesWonderland.Services.QuestService;
using UserServiceImpl = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Missions;

/// <summary>
/// The M3 long tail: hidden stories collected, encyclopedia entries, costume ability level, favorite character,
/// transfer settings, and quest battles quit or lost.
/// </summary>
public class LongTailMissionTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
    private static long NowMs => Now.ToUnixTimeMilliseconds();

    private MissionTracker CreateTracker() => new(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Load(MissionFilterTable.DefaultPath)));

    private static EntityIUserMission Row(DarkUserMemoryDatabase userDb, int missionId) => userDb.EntityIUserMission.Single(r => r.MissionId == missionId);

    [Fact]
    public void StateConditions_ReproduceTheRealSave()
    {
        DarkUserMemoryDatabase save = new UserDataSeeder(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } })).LoadFromFiles();
        // The last "Collect N Hidden Stories" (210), "Add N entries to your encyclopedia" (300), character ability
        // level 3 and "Set a favorite character": drop the save's rows and let the tracker rebuild them.
        int[] missionIds = [490050, 350030, 1130, 11];
        Dictionary<int, int> saved = save.EntityIUserMission.Where(r => missionIds.Contains(r.MissionId)).ToDictionary(r => r.MissionId, r => r.ProgressValue);
        save.EntityIUserMission.RemoveAll(r => missionIds.Contains(r.MissionId));

        MissionTracker tracker = CreateTracker();
        tracker.Refresh(save, NowMs);
        // The ability-level missions come with a shop purchase (unlock GRANT), so grant their group as the shop does.
        tracker.GrantGroup(save, MasterDb.EntityMMission.Single(m => m.MissionId == 1130).MissionGroupId, NowMs);

        Assert.Equal(210, Row(save, 490050).ProgressValue);
        Assert.True(Row(save, 350030).ProgressValue >= saved[350030], $"encyclopedia {Row(save, 350030).ProgressValue}");
        Assert.Equal(saved[1130], Row(save, 1130).ProgressValue);
        Assert.Equal(1, Row(save, 11).ProgressValue);
        Assert.All(missionIds, id => Assert.Equal(MissionProgressStatusType.CLEAR, Row(save, id).MissionProgressStatusType));
    }

    [Fact]
    public void QuestBattleLoss_CountsQuitsAndWipesApart()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        MissionTracker tracker = CreateTracker();
        tracker.Refresh(userDb, NowMs);

        tracker.RecordQuestBattleLoss(userDb, isRetired: true, isAnnihilated: false, NowMs);
        tracker.RecordQuestBattleLoss(userDb, isRetired: false, isAnnihilated: true, NowMs);
        tracker.RecordQuestBattleLoss(userDb, isRetired: false, isAnnihilated: true, NowMs);

        Assert.Equal(1, Row(userDb, 440001).ProgressValue);  // "Quit battles 100 times"
        Assert.Equal(2, Row(userDb, 450001).ProgressValue);  // "Lose your whole party in battle 100 times"
    }

    [Fact]
    public async Task CheckTransferSetting_ClearsTheTransferMission()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("long-tail-transfer-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        MissionTracker tracker = CreateTracker();
        tracker.Refresh(userDb, NowMs);
        UserServiceImpl service = new(store, new UserDataSeeder(Options.Create(new ServerOptions())), MasterDb, tracker, new FakeTimeProvider(Now));

        await service.CheckTransferSetting(new Empty(), ContextFor(userId));

        Assert.Equal(MissionProgressStatusType.CLEAR, Row(userDb, 12).MissionProgressStatusType);
    }

    [Fact]
    public async Task UpdateMissionProgress_CageValuesCountForTheCageMissions()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("long-tail-cage-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        MissionTracker tracker = CreateTracker();
        tracker.Refresh(userDb, NowMs);
        MissionServiceImpl service = new(store, MasterDb, tracker, new FakeTimeProvider(Now));
        UpdateMissionProgressRequest report = new() { CageMeasurableValues = new CageMeasurableValues { RunningDistanceMeters = 600, MamaTappedCount = 30 } };

        await service.UpdateMissionProgress(report, ContextFor(userId));
        await service.UpdateMissionProgress(report, ContextFor(userId));

        Assert.Equal(MissionProgressStatusType.CLEAR, Row(userDb, 500047).MissionProgressStatusType);  // "Walk 1 km in The Cage"
        Assert.Equal(60, Row(userDb, 500021).ProgressValue);                                            // "Tap Mama 50 times"
    }

    [Theory]
    [InlineData(true, false, 440001)]
    [InlineData(false, true, 450001)]
    public async Task FinishMainQuest_QuitOrWipe_CountsForItsMission(bool isRetired, bool isAnnihilated, int missionId)
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("long-tail-loss-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        MissionTracker tracker = CreateTracker();
        tracker.Refresh(userDb, NowMs);
        QuestServiceImpl service = new(store, MasterDb, TestServices.QuestEngine(MasterDb, tracker), new QuestCampaignHelper(MasterDb), tracker, new FakeTimeProvider(Now));
        HashSet<int> mainQuestIds = [.. MasterDb.EntityMMainQuestSequence.Select(s => s.QuestId)];
        EntityMQuest quest = MasterDb.EntityMQuest.Where(q => mainQuestIds.Contains(q.QuestId) && !q.IsRunInTheBackground && q.IsCountedAsQuest).OrderBy(q => q.QuestId).First();

        await service.StartMainQuest(new StartMainQuestRequest { QuestId = quest.QuestId, IsMainFlow = true, UserDeckNumber = 1 }, ContextFor(userId));
        await service.FinishMainQuest(new FinishMainQuestRequest { QuestId = quest.QuestId, IsMainFlow = true, IsRetired = isRetired, IsAnnihilated = isAnnihilated }, ContextFor(userId));

        Assert.Equal(1, Row(userDb, missionId).ProgressValue);
    }
}
