using System.Text.Json;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;

namespace MariesWonderland.Tests.Missions;

/// <summary>The filter fields mapped from the review: weapons, costumes, explorations, quest kinds and mission-keyed entries.</summary>
public class MissionFilterFieldTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private MissionCatalog Catalog => new(MasterDb, MissionFilterTable.Load(MissionFilterTable.DefaultPath));

    private (MissionTracker Tracker, MissionCatalog Catalog, DarkUserMemoryDatabase UserDb) Create()
    {
        MissionCatalog catalog = Catalog;
        DarkUserMemoryDatabase userDb = CreateUserDb();
        return (new MissionTracker(MasterDb, catalog), catalog, userDb);
    }

    private static int Progress(DarkUserMemoryDatabase userDb, MissionDefinition mission)
        => userDb.EntityIUserMission.Single(r => r.MissionId == mission.MissionId).ProgressValue;

    [Fact]
    public void FilterTable_EveryIdPointsAtRealMasterData()
    {
        MissionCatalog catalog = Catalog;
        HashSet<int> questIds = [.. MasterDb.EntityMQuest.Select(q => q.QuestId)];
        HashSet<int> weaponIds = [.. MasterDb.EntityMWeapon.Select(w => w.WeaponId)];
        HashSet<int> costumeIds = [.. MasterDb.EntityMCostume.Select(c => c.CostumeId)];
        HashSet<int> chapterIds = [.. MasterDb.EntityMEventQuestChapter.Select(c => c.EventQuestChapterId)];
        HashSet<int> exploreIds = [.. MasterDb.EntityMExplore.Select(e => e.ExploreId)];

        foreach (MissionFilter filter in catalog.All.Where(m => m.Filter != null).Select(m => m.Filter!).Distinct())
        {
            Assert.All(filter.QuestIds ?? [], id => Assert.Contains(id, questIds));
            Assert.All(filter.WeaponIds ?? [], id => Assert.Contains(id, weaponIds));
            Assert.All(filter.CostumeIds ?? [], id => Assert.Contains(id, costumeIds));
            Assert.All(filter.EventQuestChapterIds ?? [], id => Assert.Contains(id, chapterIds));
            Assert.All(filter.ExploreIds ?? [], id => Assert.Contains(id, exploreIds));
        }
    }

    [Fact]
    public void MissionEntry_WinsOverItsOptionGroup()
    {
        MissionCatalog catalog = Catalog;

        // Group 30030 is guerrilla quests throughout: the November 2021 missions say "main quest" in English, but the
        // Japanese text (ゲリラクエスト) shows they're guerrilla too, so they have no entry of their own.
        Assert.Equal(EventQuestType.GUERRILLA, catalog.Find(5801)!.Filter!.EventQuestType);
        Assert.Equal(EventQuestType.GUERRILLA, catalog.Find(5101)!.Filter!.EventQuestType);
        // Group 200013: the 2023 re-run mission points at the re-run's quest, the original keeps the original's.
        Assert.Equal([200280], catalog.Find(28501)!.Filter!.QuestIds!);
        Assert.Equal([200010], catalog.Find(2701)!.Filter!.QuestIds!);
    }

    [Fact]
    public void QuestFilter_MatchesTypeListsOncePerDayAndDeckCostume()
    {
        QuestCampaignTarget special = new(400001, QuestType.EVENT_QUEST, 11, EventQuestType.SPECIAL, DifficultyType.NORMAL);
        MissionQuestEvent oncePerDay = new(special, [1001], [24004], DailyClearableCount: 1);
        MissionQuestEvent unlimited = new(special, [1001], [24004]);

        Assert.True(new MissionFilter(EventQuestType: EventQuestType.SPECIAL, DailyClearableCount: 1).MatchesQuest(oncePerDay));
        Assert.False(new MissionFilter(EventQuestType: EventQuestType.SPECIAL, DailyClearableCount: 1).MatchesQuest(unlimited));
        Assert.True(new MissionFilter(EventQuestTypes: [EventQuestType.MARATHON, EventQuestType.SPECIAL]).MatchesQuest(unlimited));
        Assert.False(new MissionFilter(EventQuestTypes: [EventQuestType.END_CONTENTS, EventQuestType.LIMIT_CONTENT]).MatchesQuest(unlimited));
        Assert.True(new MissionFilter(EventQuestChapterIds: [10, 11]).MatchesQuest(unlimited));
        Assert.False(new MissionFilter(EventQuestChapterIds: [12]).MatchesQuest(unlimited));
        Assert.True(new MissionFilter(CostumeIds: [24004]).MatchesQuest(unlimited));
        Assert.False(new MissionFilter(CostumeIds: [24003]).MatchesQuest(unlimited));
        Assert.False(new MissionFilter(CostumeIds: [24004]).MatchesQuest(new MissionQuestEvent(special, [1001])));
        // "Main quests on Hard or Very Hard"
        MissionFilter hardOrVeryHard = new(QuestType: QuestType.MAIN_QUEST, Difficulties: [DifficultyType.HARD, DifficultyType.VERY_HARD]);
        Assert.True(hardOrVeryHard.MatchesQuest(new MissionQuestEvent(new QuestCampaignTarget(10050, QuestType.MAIN_QUEST, 7, Difficulty: DifficultyType.HARD), [])));
        Assert.True(hardOrVeryHard.MatchesQuest(new MissionQuestEvent(new QuestCampaignTarget(20050, QuestType.MAIN_QUEST, 7, Difficulty: DifficultyType.VERY_HARD), [])));
        Assert.False(hardOrVeryHard.MatchesQuest(new MissionQuestEvent(new QuestCampaignTarget(50, QuestType.MAIN_QUEST, 7, Difficulty: DifficultyType.NORMAL), [])));
        // A filter meant for weapons never matches a quest clear.
        Assert.False(new MissionFilter(WeaponIds: [250011]).MatchesQuest(unlimited));
    }

    [Fact]
    public void RhythmTaps_CountTheBestRunOfTheNamedConcert()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition maxExcitement = catalog.Find(460007)!;  // live type 2, 43 notes
        MissionDefinition perfect = catalog.Find(460008)!;        // live type 3, 33 notes
        long atMs = Math.Max(maxExcitement.StartDatetime, perfect.StartDatetime) + 1;
        tracker.GrantGroup(userDb, maxExcitement.MissionGroupId, atMs);

        tracker.RecordRhythmTaps(userDb, 3, 30, atMs);
        Assert.Equal(0, Progress(userDb, maxExcitement));
        Assert.Equal(30, Progress(userDb, perfect));

        // A worse run doesn't lower the best; a perfect run clears it.
        tracker.RecordRhythmTaps(userDb, 3, 12, atMs);
        Assert.Equal(30, Progress(userDb, perfect));
        tracker.RecordRhythmTaps(userDb, 3, 33, atMs);
        Assert.Equal(MissionProgressStatusType.CLEAR, userDb.EntityIUserMission.Single(r => r.MissionId == perfect.MissionId).MissionProgressStatusType);

        tracker.RecordRhythmTaps(userDb, 2, 43, atMs);
        Assert.Equal(MissionProgressStatusType.CLEAR, userDb.EntityIUserMission.Single(r => r.MissionId == maxExcitement.MissionId).MissionProgressStatusType);
    }

    [Fact]
    public void TitleTransitions_CountWithinOneGameDay()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition titleFiveTimes = catalog.Find(7007)!;  // "See the title screen 5 times in a day", empty filter
        const long DayMs = 24L * 60 * 60 * 1000;
        long dayOne = GameDayHelper.StartOfDay(titleFiveTimes.StartDatetime) + DayMs + 1000;
        tracker.GrantGroup(userDb, titleFiveTimes.MissionGroupId, dayOne);

        for (int i = 0; i < 4; i++)
        {
            tracker.RecordTitleTransition(userDb, dayOne, dayOne);
        }

        Assert.Equal(4, Progress(userDb, titleFiveTimes));

        // Four views one day and one the next don't add up to five in a day.
        long dayTwo = dayOne + DayMs;
        tracker.RecordTitleTransition(userDb, dayOne, dayTwo);
        Assert.Equal(1, Progress(userDb, titleFiveTimes));
    }

    [Fact]
    public void WeaponEvent_CountsOnlyForTheNamedWeapon()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition ascend = catalog.ByConditionType(MissionClearConditionType.WEAPON_LIMIT_BREAK_BY_COUNT).First(m => m.Filter?.WeaponIds != null);
        long atMs = ascend.StartDatetime + 1;
        tracker.GrantGroup(userDb, ascend.MissionGroupId, atMs);
        int named = ascend.Filter!.WeaponIds![0];
        int other = MasterDb.EntityMWeapon.First(w => !ascend.Filter.WeaponIds.Contains(w.WeaponId)).WeaponId;

        tracker.RecordWeapon(userDb, MissionClearConditionType.WEAPON_LIMIT_BREAK_BY_COUNT, other, 1, atMs);
        Assert.Equal(0, Progress(userDb, ascend));
        tracker.RecordWeapon(userDb, MissionClearConditionType.WEAPON_LIMIT_BREAK_BY_COUNT, named, 1, atMs);
        Assert.Equal(1, Progress(userDb, ascend));
    }

    [Fact]
    public void EvolveMissions_NameOnlyStagesThatCanEvolve()
    {
        HashSet<int> canEvolve = [.. MasterDb.EntityMWeaponEvolutionGroup
            .Where(e => MasterDb.EntityMWeaponEvolutionGroup.Any(n => n.WeaponEvolutionGroupId == e.WeaponEvolutionGroupId && n.EvolutionOrder == e.EvolutionOrder + 1))
            .Select(e => e.WeaponId)];
        List<MissionDefinition> evolve = [.. Catalog.ByConditionType(MissionClearConditionType.WEAPON_EVOLVE_BY_COUNT).Where(m => m.Filter?.WeaponIds != null)];

        Assert.NotEmpty(evolve);
        Assert.All(evolve, m => Assert.All(m.Filter!.WeaponIds!, id => Assert.Contains(id, canEvolve)));
    }

    [Fact]
    public void WeaponMaxLevel_ReadsTheNamedWeaponOnly()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition level = catalog.ByConditionType(MissionClearConditionType.WEAPON_MAX_LEVEL).First(m => m.Filter?.WeaponIds != null && m.ClearValue > 5);
        long atMs = level.StartDatetime + 1;
        int named = level.Filter!.WeaponIds![^1];
        int other = MasterDb.EntityMWeapon.First(w => !level.Filter.WeaponIds.Contains(w.WeaponId)).WeaponId;
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon { UserWeaponUuid = "other", WeaponId = other, Level = 90 });
        tracker.GrantGroup(userDb, level.MissionGroupId, atMs);
        Assert.Equal(0, Progress(userDb, level));

        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon { UserWeaponUuid = "named", WeaponId = named, Level = 4 });
        tracker.EvaluateStates(userDb, atMs);
        Assert.Equal(4, Progress(userDb, level));
    }

    [Fact]
    public void CostumeEvent_CountsOnlyForTheNamedCostume()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        MissionDefinition ascend = catalog.ByConditionType(MissionClearConditionType.COSTUME_LIMIT_BREAK_BY_COUNT).First(m => m.Filter?.CostumeIds != null);
        long atMs = ascend.StartDatetime + 1;
        tracker.GrantGroup(userDb, ascend.MissionGroupId, atMs);

        tracker.RecordCostume(userDb, MissionClearConditionType.COSTUME_LIMIT_BREAK_BY_COUNT, 10100, 1, atMs);
        Assert.Equal(0, Progress(userDb, ascend));
        tracker.RecordCostume(userDb, MissionClearConditionType.COSTUME_LIMIT_BREAK_BY_COUNT, ascend.Filter!.CostumeIds![0], 1, atMs);
        Assert.Equal(1, Progress(userDb, ascend));
    }

    [Fact]
    public void ExploreHighScore_ReadsTheNamedExplorationOnly()
    {
        (MissionTracker tracker, MissionCatalog catalog, DarkUserMemoryDatabase userDb) = Create();
        // "Get rank A in Shooting": Normal Shooting only (explore id 1).
        MissionDefinition shooting = catalog.ByConditionType(MissionClearConditionType.EXPLORE_HIGH_SCORE)
            .First(m => m.Filter?.ExploreIds is [1]);
        long atMs = shooting.StartDatetime + 1;
        userDb.EntityIUserExploreScore.Add(new EntityIUserExploreScore { ExploreId = 11, MaxScore = 200000 });
        tracker.GrantGroup(userDb, shooting.MissionGroupId, atMs);
        Assert.Equal(0, Progress(userDb, shooting));

        userDb.EntityIUserExploreScore.Add(new EntityIUserExploreScore { ExploreId = 1, MaxScore = 4321 });
        tracker.EvaluateStates(userDb, atMs);
        Assert.Equal(4321, Progress(userDb, shooting));
    }

    [Fact]
    public async Task BoardPanels_CountOncePerNewPanelOnTheNamedGroup()
    {
        // "Unlock N Stone Tower Monument panels for Rion" (option group 310001) is board group 100401.
        MissionFilterTable filters = new(new Dictionary<int, MissionFilter> { [310001] = new(CharacterBoardGroupId: 100401) });
        MissionCatalog catalog = new(MasterDb, filters);
        MissionDefinition rion = catalog.ByConditionType(MissionClearConditionType.CHARACTER_BOARD_PANEL_RELEASE_BY_COUNT)
            .First(m => m.Mission.MissionClearConditionOptionGroupId == 310001);
        MissionTracker tracker = new(MasterDb, catalog);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        UserDataStore store = CreateStore(1, userDb);
        long atMs = rion.StartDatetime + 1;
        tracker.GrantGroup(userDb, rion.MissionGroupId, atMs);
        MariesWonderland.Services.CharacterBoardService service = new(MasterDb, store, tracker, new FakeTimeProvider(DateTimeOffset.FromUnixTimeMilliseconds(atMs)));

        int[] boards = [.. MasterDb.EntityMCharacterBoard.Where(b => b.CharacterBoardGroupId == 100401).Select(b => b.CharacterBoardId)];
        int[] panels = [.. MasterDb.EntityMCharacterBoardPanel.Where(p => boards.Contains(p.CharacterBoardId)).Take(2).Select(p => p.CharacterBoardPanelId)];
        int otherGroupPanel = MasterDb.EntityMCharacterBoardPanel.First(p => !boards.Contains(p.CharacterBoardId)).CharacterBoardPanelId;

        await service.ReleasePanel(new Proto.CharacterBoard.ReleasePanelRequest { CharacterBoardPanelId = { panels[0], panels[1], otherGroupPanel } }, ContextFor(1));
        Assert.Equal(2, Progress(userDb, rion));
        // Releasing a panel that is already released counts nothing.
        await service.ReleasePanel(new Proto.CharacterBoard.ReleasePanelRequest { CharacterBoardPanelId = { panels[0] } }, ContextFor(1));
        Assert.Equal(2, Progress(userDb, rion));
    }

    [Fact]
    public void GachaFilter_MatchesLabelBannerAndDailyPull()
    {
        MissionFilter chapter = new(GachaLabelType: GachaLabelType.CHAPTER);
        MissionFilter daily = new(IsDailyFreeDraw: true);
        // "Mama's Exciting Festival Multi-Step Summons": the step-up listed as group 552, first step 552001.
        MissionFilter festival = new(GachaIds: [552001]);

        Assert.True(chapter.MatchesGacha(GachaLabelType.CHAPTER, [200001], isDailyFreeDraw: false));
        Assert.False(chapter.MatchesGacha(GachaLabelType.PREMIUM, [468], isDailyFreeDraw: false));
        Assert.False(chapter.MatchesGacha(null, [], isDailyFreeDraw: true));
        Assert.True(daily.MatchesGacha(null, [], isDailyFreeDraw: true));
        Assert.False(daily.MatchesGacha(GachaLabelType.PREMIUM, [468], isDailyFreeDraw: false));
        Assert.True(festival.MatchesGacha(GachaLabelType.PREMIUM, [552, 552001], isDailyFreeDraw: false));
        Assert.False(festival.MatchesGacha(GachaLabelType.PREMIUM, [553, 553001], isDailyFreeDraw: false));
        // Chapter summons are told apart by ticket: main chapter tickets vs the Dark Memories Summon Ticket (2002).
        MissionFilter darkMemory = new(GachaLabelType: GachaLabelType.CHAPTER, GachaPriceItemIds: [2002]);
        Assert.True(darkMemory.MatchesGacha(GachaLabelType.CHAPTER, [1], isDailyFreeDraw: false, priceItemId: 2002));
        Assert.False(darkMemory.MatchesGacha(GachaLabelType.CHAPTER, [1], isDailyFreeDraw: false, priceItemId: 1011));
        Assert.False(festival.MatchesQuest(new MissionQuestEvent(new QuestCampaignTarget(1, QuestType.MAIN_QUEST), [])));
    }

    [Fact]
    public void ShopFilter_MatchesTheItemShopOnly()
    {
        MissionFilter itemShop = new(ShopType: ShopType.ITEM_SHOP);

        Assert.True(itemShop.MatchesShop(ShopType.ITEM_SHOP));
        Assert.False(itemShop.MatchesShop(ShopType.EXCHANGE_SHOP));
        Assert.True(new MissionFilter().MatchesShop(ShopType.GOLD_SHOP));
    }

    [Fact]
    public void ProfileMessage_ContainsTheWord()
    {
        MissionFilter newYear = new(MessageWords: ["A Happy New Year", "あけましておめでとう", "새해 복 많이 받으세요"]);

        Assert.True(newYear.MatchesMessage("A Happy New Year"));
        Assert.True(newYear.MatchesMessage("皆さん、あけましておめでとう！"));
        Assert.True(newYear.MatchesMessage("새해 복 많이 받으세요"));
        Assert.True(newYear.MatchesMessage("Wishing everyone A Happy New Year!"));
        Assert.False(newYear.MatchesMessage("a happy new year"));
        Assert.False(newYear.MatchesMessage("Happy holidays"));
        Assert.True(new MissionFilter().MatchesMessage("anything"));
    }

    [Fact]
    public void Load_RejectsUnknownFieldsAndAmbiguousKeys()
    {
        string path = Path.Combine(Path.GetTempPath(), $"mission-filters-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """{ "optionGroups": [ { "optionGroupId": 1, "weaponId": [250011] } ] }""");
            Assert.ThrowsAny<JsonException>(() => MissionFilterTable.Load(path));

            File.WriteAllText(path, """{ "optionGroups": [ { "optionGroupId": 1, "missionId": 2, "questIds": [1] } ] }""");
            Assert.Throws<InvalidDataException>(() => MissionFilterTable.Load(path));

            File.WriteAllText(path, """{ "optionGroups": [ { "optionGroupId": 1, "questIds": [1] }, { "missionId": 5, "questType": "MAIN_QUEST" } ] }""");
            MissionFilterTable table = MissionFilterTable.Load(path);
            Assert.Equal(QuestType.MAIN_QUEST, table.FindForMission(5, 1)!.QuestType);
            Assert.Equal([1], table.FindForMission(6, 1)!.QuestIds!);
            Assert.Null(table.FindForMission(7, 0));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
