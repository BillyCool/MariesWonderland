using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using UserService = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Missions;

/// <summary>Mission counter events (milestone 2): what the services report and how filters pick missions.</summary>
public class MissionEventTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
    private static long NowMs => Now.ToUnixTimeMilliseconds();

    private MissionCatalog Catalog => new(MasterDb, MissionFilterTable.Load(MissionFilterTable.DefaultPath));

    private (MissionTracker Tracker, MissionCatalog Catalog, DarkUserMemoryDatabase UserDb, UserDataStore Store, long UserId) Create()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("mission-event-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        userDb.EntityIUserMission.Clear();
        MissionCatalog catalog = Catalog;
        return (new MissionTracker(MasterDb, catalog), catalog, userDb, store, userId);
    }

    /// <summary>The first open mission of a condition type matching <paramref name="where"/> that a fresh account gets at login.</summary>
    private static MissionDefinition Open(MissionCatalog catalog, MissionClearConditionType type, Func<MissionDefinition, bool> where)
        => catalog.ByConditionType(type).First(m => m.IsInTerm(NowMs)
            && m.UnlockType is MissionUnlockConditionType.UNKNOWN or MissionUnlockConditionType.EVALUATE && m.UnlockValue == 0 && where(m));

    private static int Progress(DarkUserMemoryDatabase userDb, MissionDefinition mission)
        => userDb.EntityIUserMission.Single(r => r.MissionId == mission.MissionId).ProgressValue;

    [Fact]
    public void FilterTable_EveryEntryPointsAtRealMasterData()
    {
        MissionCatalog catalog = Catalog;
        HashSet<int> characterIds = [.. MasterDb.EntityMCharacter.Select(c => c.CharacterId)];
        HashSet<int> eventChapterIds = [.. MasterDb.EntityMEventQuestChapter.Select(c => c.EventQuestChapterId)];

        foreach (MissionFilter filter in catalog.All.Where(m => m.Filter != null).Select(m => m.Filter!).Distinct())
        {
            Assert.True(filter.CharacterId == null || characterIds.Contains(filter.CharacterId.Value));
            Assert.True(filter.EventQuestChapterId == null || eventChapterIds.Contains(filter.EventQuestChapterId.Value));
        }
    }

    [Fact]
    public void QuestFilter_MatchesTypeChapterDifficultyAndDeckCharacter()
    {
        QuestCampaignTarget guerrillaHard = new(1, QuestType.EVENT_QUEST, 7, EventQuestType.GUERRILLA, DifficultyType.HARD);
        MissionQuestEvent withLars = new(guerrillaHard, [1013]);

        Assert.True(new MissionFilter(EventQuestType: EventQuestType.GUERRILLA).MatchesQuest(withLars));
        Assert.True(new MissionFilter(EventQuestChapterId: 7, Difficulty: DifficultyType.HARD).MatchesQuest(withLars));
        Assert.True(new MissionFilter(CharacterId: 1013).MatchesQuest(withLars));
        Assert.False(new MissionFilter(CharacterId: 1008).MatchesQuest(withLars));
        Assert.False(new MissionFilter(QuestType: QuestType.MAIN_QUEST).MatchesQuest(withLars));
        Assert.False(new MissionFilter(EventQuestType: EventQuestType.END_CONTENTS).MatchesQuest(withLars));
        Assert.False(new MissionFilter(Difficulty: DifficultyType.VERY_HARD).MatchesQuest(withLars));
        Assert.False(new MissionFilter(MainQuestChapterId: 7).MatchesQuest(withLars));
    }

    [Fact]
    public void QuestClear_CountsUnfilteredAndMatchingMissionsOnly()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb, _, _) = Create();
        // Account mission 500056: Quest 10 of Chapter 4 on Very Hard with only Akeha, skip tickets not allowed.
        MissionDefinition withoutSkip = catalog.Find(500056)!;
        long atMs = NowMs;
        MissionDefinition any = catalog.ByConditionType(MissionClearConditionType.QUEST_CLEAR_BY_COUNT)
            .First(m => m.MissionGroupId != withoutSkip.MissionGroupId && m.Filter == null && !m.IsFilterUnmapped && m.ClearValue > 3 && m.IsInTerm(atMs));
        tracker.GrantGroup(userDb, withoutSkip.MissionGroupId, atMs);
        tracker.GrantGroup(userDb, any.MissionGroupId, atMs);
        MissionQuestEvent quest = new(new QuestCampaignTarget(20034, QuestType.MAIN_QUEST, 5, Difficulty: DifficultyType.VERY_HARD), [1015]);

        tracker.RecordQuestClear(userDb, quest, 1, isSkipped: false, atMs);
        tracker.RecordQuestClear(userDb, quest, 2, isSkipped: true, atMs);

        Assert.Equal(3, Progress(userDb, any));
        Assert.Equal(1, Progress(userDb, withoutSkip));
    }

    [Fact]
    public void Engine_CountsStaminaEvenWhenFree_AndQuestWins()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb, _, _) = Create();
        tracker.Refresh(userDb, NowMs);
        QuestCampaignHelper campaigns = new(MasterDb);
        QuestEngine engine = new(MasterDb, GameConfig, campaigns, new ImportantItemEffectHelper(MasterDb), tracker, Options.Create(new DevModeOptions { FreeStamina = true }));
        EntityMQuest quest = MasterDb.EntityMQuest.Where(q => q.Stamina > 0).OrderBy(q => q.QuestId).First();
        MissionDefinition stamina = Open(catalog, MissionClearConditionType.CONSUME_STAMINA_AMOUNT, m => m.Filter == null);
        MissionDefinition clears = Open(catalog, MissionClearConditionType.QUEST_CLEAR_BY_COUNT, m => m.Filter == null && !m.IsFilterUnmapped && m.ClearValue > 1);
        int staminaBefore = userDb.EntityIUserStatus.Single().StaminaMilliValue;

        engine.ConsumeStamina(userDb, quest, campaigns.GetMainQuestTarget(quest.QuestId), 2, NowMs);
        engine.Finish(userDb, quest, campaigns.GetMainQuestTarget(quest.QuestId), isWin: true, NowMs);
        engine.Finish(userDb, quest, campaigns.GetMainQuestTarget(quest.QuestId), isWin: false, NowMs);

        Assert.Equal(staminaBefore, userDb.EntityIUserStatus.Single().StaminaMilliValue);
        Assert.Equal(quest.Stamina * 2, Progress(userDb, stamina));
        Assert.Equal(1, Progress(userDb, clears));
    }

    [Fact]
    public async Task GameStart_CountsOneLoginPerGameDay()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb, UserDataStore store, long userId) = Create();
        FakeTimeProvider time = new(Now);
        UserService service = new(store, new UserDataSeeder(Options.Create(new ServerOptions())), MasterDb, tracker, time);
        MissionDefinition login = Open(catalog, MissionClearConditionType.USER_LOGIN_BY_COUNT, m => m.Category == MissionCategoryType.CHALLENGE && m.ClearValue > 2);
        // Last seen two days ago, on a 5-day login run.
        long twoDaysAgo = Now.AddDays(-2).ToUnixTimeMilliseconds();
        userDb.EntityIUser.Single().GameStartDatetime = twoDaysAgo;
        EntityIUserLogin record = userDb.EntityIUserLogin.Single();
        record.LastLoginDatetime = twoDaysAgo;
        record.TotalLoginCount = 10;
        record.ContinualLoginCount = 5;
        record.MaxContinualLoginCount = 5;

        await service.GameStart(new Empty(), ContextFor(userId));
        await service.GameStart(new Empty(), ContextFor(userId));
        Assert.Equal(1, Progress(userDb, login));
        Assert.Equal((11, 1), (record.TotalLoginCount, record.ContinualLoginCount));

        time.SetUtcNow(Now.AddDays(1));
        await service.GameStart(new Empty(), ContextFor(userId));
        Assert.Equal(2, Progress(userDb, login));
        Assert.Equal((12, 2, 5), (record.TotalLoginCount, record.ContinualLoginCount, record.MaxContinualLoginCount));
    }
}
