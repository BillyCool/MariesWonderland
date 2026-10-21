using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Missions;

/// <summary>
/// The account missions (EVALUATE category, ids 500001–500186): the hidden-story prerequisites. They are ordinary
/// missions, available from the start; these tests cover the conditions and filters only they needed.
/// </summary>
public class AccountMissionTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long Now = 1_700_000_000_000;

    private const int Lars = 1013, Griff = 1014, Akeha = 1015, Rion = 1008;

    private MissionCatalog Catalog => new(MasterDb, MissionFilterTable.Load(MissionFilterTable.DefaultPath));

    private (MissionTracker Tracker, DarkUserMemoryDatabase UserDb) Create()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        MissionTracker tracker = new(MasterDb, Catalog);
        tracker.Refresh(userDb, Now);
        return (tracker, userDb);
    }

    private static int Progress(DarkUserMemoryDatabase userDb, int missionId)
        => userDb.EntityIUserMission.Single(r => r.MissionId == missionId).ProgressValue;

    private static MissionProgressStatusType Status(DarkUserMemoryDatabase userDb, int missionId)
        => userDb.EntityIUserMission.Single(r => r.MissionId == missionId).MissionProgressStatusType;

    [Fact]
    public void AccountMissions_AreAvailableFromTheStart_AndThoseNamingALoadoutHaveAFilter()
    {
        (_, DarkUserMemoryDatabase userDb) = Create();
        MissionCatalog catalog = Catalog;
        List<MissionDefinition> account = [.. catalog.All.Where(m => m.Category == MissionCategoryType.EVALUATE)];

        Assert.Equal(186, account.Count);
        Assert.All(account, m => Assert.Contains(userDb.EntityIUserMission, r => r.MissionId == m.MissionId));

        // Missions without an option group that still name a quest, boss or loadout have their own entry.
        int[] named = [500013, 500045, 500056, 500057, 500058, 500059, 500067, 500068, 500078, 500084, 500085, 500087, 500089, 500102, 500108, 500109];
        Assert.All(named, id => Assert.True(catalog.Find(id)!.Filter is { IsEmpty: false }, $"mission {id}"));
    }

    [Fact]
    public void BigHuntScore_WithALoadout_CountsOnlyRunsWithThatDeck()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserBigHuntMaxScore.Add(new() { UserId = userDb.UserId, BigHuntBossId = 3, MaxScore = 20_000_000 });
        MissionTracker tracker = new(MasterDb, Catalog);
        tracker.Refresh(userDb, Now);

        // "S5 on Windy" reads the saved best; "S1 on Windy with Lars and Griff" can't, so it waits for a run.
        Assert.Equal(MissionProgressStatusType.CLEAR, Status(userDb, 500035));
        Assert.Equal(0, Progress(userDb, 500045));

        tracker.RecordBigHuntScore(userDb, 3, [Lars], 2_000_000, Now);
        Assert.Equal(0, Progress(userDb, 500045));
        tracker.RecordBigHuntScore(userDb, 1, [Lars, Griff], 2_000_000, Now);
        Assert.Equal(0, Progress(userDb, 500045));
        tracker.RecordBigHuntScore(userDb, 3, [Lars, Griff, Rion], 1_600_000, Now);
        Assert.Equal(MissionProgressStatusType.CLEAR, Status(userDb, 500045));
    }

    [Fact]
    public void OnlyCharacters_RejectsADeckWithAnyoneElse()
    {
        (MissionTracker tracker, DarkUserMemoryDatabase userDb) = Create();
        // 500056: Quest 10 of Chapter 4 (chapter id 5) on Very Hard with only Akeha, no skip tickets.
        QuestCampaignTarget quest = new(20034, QuestType.MAIN_QUEST, 5, Difficulty: DifficultyType.VERY_HARD);

        tracker.RecordQuestClear(userDb, new MissionQuestEvent(quest, [Akeha, Rion]), 1, isSkipped: false, Now);
        Assert.Equal(0, Progress(userDb, 500056));
        tracker.RecordQuestClear(userDb, new MissionQuestEvent(quest, [Akeha]), 1, isSkipped: true, Now);
        Assert.Equal(0, Progress(userDb, 500056));
        tracker.RecordQuestClear(userDb, new MissionQuestEvent(quest, [Akeha]), 1, isSkipped: false, Now);
        Assert.Equal(MissionProgressStatusType.CLEAR, Status(userDb, 500056));
    }

    [Fact]
    public void CageValues_AddUpAcrossReports()
    {
        (MissionTracker tracker, DarkUserMemoryDatabase userDb) = Create();

        // 500047 "Walk 1 km in The Cage", 500021 "Tap Mama 50 times".
        tracker.RecordCageValues(userDb, 600, 30, Now);
        Assert.Equal(600, Progress(userDb, 500047));
        tracker.RecordCageValues(userDb, 500, 25, Now);
        Assert.Equal(MissionProgressStatusType.CLEAR, Status(userDb, 500047));
        Assert.Equal(55, Progress(userDb, 500021));
        Assert.Equal(MissionProgressStatusType.CLEAR, Status(userDb, 500021));
    }

    [Fact]
    public void FinishWave_CountsTheBossesOfAWipedOutEnemyParty()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("account-mission-boss-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        MissionTracker tracker = new(MasterDb, Catalog);
        tracker.Refresh(userDb, TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds());
        MariesWonderland.Services.BattleService battle = new(store, MasterDb, tracker, TimeProvider.System);

        // NPC 1's decks 1 and 2 are three normal enemies, decks 3 and 4 a single boss (the tutorial's waves).
        FinishWaveRequest Wave(int deckNumber, int depleted) => new()
        {
            NpcPartyResultInfoList = { new NpcPartyResultInfo { NpcId = 1, DeckType = (int)DeckType.QUEST, BattleNpcDeckNumber = deckNumber, CharacterHpDepletedCount = depleted } }
        };

        battle.FinishWave(Wave(1, 3), ContextFor(userId));
        Assert.Equal(0, Progress(userDb, 500028));
        battle.FinishWave(Wave(3, 0), ContextFor(userId));
        Assert.Equal(0, Progress(userDb, 500028));
        battle.FinishWave(Wave(3, 1), ContextFor(userId));
        battle.FinishWave(Wave(4, 1), ContextFor(userId));
        Assert.Equal(2, Progress(userDb, 500028));
    }
}
