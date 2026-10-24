using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Tests.Quests;

/// <summary>Quest mission conditions checked against battle reports and the deck, and what a finish pays for them.</summary>
public class QuestMissionEvaluatorTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long NowMs = 0;

    // Main quest 8: weapon skill used once (21001), twice (21002), nobody not alive (2), and the complete bonus (1).
    private const int QuestWithSkillAndDeathMissions = 8;

    private QuestMissionEvaluator Evaluator => new(MasterDb);

    private QuestEngine CreateEngine()
        => new(MasterDb, GameConfig, new QuestCampaignHelper(MasterDb), new ImportantItemEffectHelper(MasterDb), new MissionTracker(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty)), Options.Create(new DevModeOptions()));

    private DarkUserMemoryDatabase CreateUser()
    {
        UserDataStore store = new(TimeProvider.System);
        (long userId, _) = store.RegisterOrGetUser("quest-mission-test", MasterDb);
        store.TryGet(userId, out DarkUserMemoryDatabase userDb);
        return userDb;
    }

    private static EntitySBattleDetail Battle(int weaponSkills = 0, int costumeSkills = 0, int maxDamage = 0, params (bool IsAlive, long MaxHp, long RemainingHp)[] characters)
        => new()
        {
            PlayerWeaponActiveSkillUsedCount = weaponSkills,
            PlayerCostumeActiveSkillUsedCount = costumeSkills,
            MaxDamage = maxDamage,
            CostumeBattleInfoList = [.. characters.Select(c => new CostumeBattleInfo { IsAlive = c.IsAlive, MaxHp = c.MaxHp, RemainingHp = c.RemainingHp })]
        };

    private EntityMQuestMission Mission(int missionId) => MasterDb.EntityMQuestMission.Single(m => m.QuestMissionId == missionId);

    private static EntityMQuestMission Condition(QuestMissionConditionType type, int value = 0, int groupId = 0)
        => new() { QuestMissionConditionType = type, ConditionValue = value, QuestMissionConditionValueGroupId = groupId };

    [Fact]
    public void BattleConditions_AreCheckedAgainstTheReport()
    {
        QuestRun run = Evaluator.BuildRun(CreateUser(), QuestWithSkillAndDeathMissions,
            [Battle(weaponSkills: 2, costumeSkills: 1, maxDamage: 700, (true, 1000, 800), (false, 1000, 0))]);

        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.GREATER_THAN_OR_EQUAL_X_WEAPON_SKILL_USE_COUNT, 2), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.GREATER_THAN_OR_EQUAL_X_WEAPON_SKILL_USE_COUNT, 3), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_COSTUME_SKILL_USE_COUNT, 1), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.WITHOUT_COSTUME_SKILL), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.MAX_DAMAGE, 700), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.MAX_DAMAGE, 701), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_PEOPLE_NOT_ALIVE, 1), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_PEOPLE_NOT_ALIVE, 0), run));
        // The lowest end HP of any character: the fallen one is at 0%.
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.MIN_HP_PERCENTAGE_GE, 80), run));
    }

    [Fact]
    public void MinHp_IsTheLowestEndHpOfAnyCharacter()
    {
        QuestRun run = Evaluator.BuildRun(CreateUser(), QuestWithSkillAndDeathMissions,
            [Battle(characters: [(true, 1000, 900), (true, 2000, 1700)])]);

        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.MIN_HP_PERCENTAGE_GE, 85), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.MIN_HP_PERCENTAGE_GE, 90), run));
    }

    [Fact]
    public void WithoutABattleReport_BattleConditionsAreNotMet()
    {
        QuestRun run = Evaluator.BuildRun(CreateUser(), QuestWithSkillAndDeathMissions, []);

        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_PEOPLE_NOT_ALIVE, 5), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.WITHOUT_WEAPON_SKILL), run));
    }

    [Fact]
    public void DeckConditions_AreCheckedAgainstTheDeck()
    {
        // Character group 10101 is characters 1008 and 1009; weapon type group 11101 is types 1 and 5.
        QuestRunDeck deck = new([1008, 1009, 1004], [AttributeType.FIRE, AttributeType.FIRE, AttributeType.FIRE], [WeaponType.SWORD, WeaponType.STAFF, WeaponType.GUN]);
        QuestRun run = new(null, deck);

        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.SPECIFIED_CHARACTER_IS_IN_DECK, 1004), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.SPECIFIED_CHARACTER_IS_IN_DECK, 1007), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.CHARACTER_CONTAIN_ALL, groupId: 10101), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.SPECIFIED_ATTRIBUTE_MAIN_WEAPON_IS_IN_DECK, (int)AttributeType.FIRE), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.SPECIFIED_ATTRIBUTE_MAIN_WEAPON_IS_IN_DECK, (int)AttributeType.WATER), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.SPECIFIED_ATTRIBUTE_MAIN_WEAPON_ALL_CHARACTER, (int)AttributeType.FIRE), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.COSTUME_SKILLFUL_WEAPON_ANY_CHARACTER, (int)WeaponType.GUN), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.COSTUME_SKILLFUL_WEAPON_CONTAIN_ALL, groupId: 11101), run));
        Assert.True(Evaluator.IsMet(Condition(QuestMissionConditionType.DECK_COSTUME_NUM_LE, 3), run));
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.DECK_COSTUME_NUM_LE, 2), run));
        // Without a deck, no deck condition is met.
        Assert.False(Evaluator.IsMet(Condition(QuestMissionConditionType.SPECIFIED_CHARACTER_IS_IN_DECK, 1004), new QuestRun(null, null)));
    }

    [Fact]
    public void Finish_PaysOnlyTheMissionsTheBattleMet_AndTheRestOnALaterRun()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = MasterDb.EntityMQuest.Single(q => q.QuestId == QuestWithSkillAndDeathMissions);
        QuestCampaignTarget target = new(quest.QuestId, QuestType.MAIN_QUEST);
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.EnsureQuestRows(userDb, quest.QuestId);

        // One weapon skill, nobody down: 21001 and "nobody down" are met, 21002 and the bonus are not.
        QuestFinishResult first = engine.Finish(userDb, quest, target, isWin: true, NowMs, battles: [Battle(weaponSkills: 1, characters: (true, 1000, 500))]);

        Assert.Equal(2, first.MissionClearRewards.Count);
        Assert.Empty(first.MissionClearCompleteRewards);
        Assert.False(first.IsBigWin);
        Assert.True(Cleared(userDb, quest, 21001));
        Assert.True(Cleared(userDb, quest, 2));
        Assert.False(Cleared(userDb, quest, 21002));
        Assert.False(Cleared(userDb, quest, 1));

        // Two skills this time: only the missing mission pays, and it completes the quest.
        QuestFinishResult second = engine.Finish(userDb, quest, target, isWin: true, NowMs, battles: [Battle(weaponSkills: 2, characters: (true, 1000, 500))]);

        Assert.Single(second.MissionClearRewards);
        Assert.Single(second.MissionClearCompleteRewards);
        Assert.True(second.IsBigWin);
        Assert.Equal(1, second.BigWinMissionId);
        Assert.True(Cleared(userDb, quest, 21002));
        Assert.True(Cleared(userDb, quest, 1));
    }

    [Fact]
    public void Finish_WithoutBattleReports_ClearsNothing()
    {
        QuestEngine engine = CreateEngine();
        EntityMQuest quest = MasterDb.EntityMQuest.Single(q => q.QuestId == QuestWithSkillAndDeathMissions);
        DarkUserMemoryDatabase userDb = CreateUser();
        engine.EnsureQuestRows(userDb, quest.QuestId);

        QuestFinishResult result = engine.Finish(userDb, quest, new QuestCampaignTarget(quest.QuestId, QuestType.MAIN_QUEST), isWin: true, NowMs, battles: []);

        Assert.Empty(result.MissionClearRewards);
        Assert.False(result.IsBigWin);
        Assert.DoesNotContain(userDb.EntityIUserQuestMission, m => m.QuestId == quest.QuestId && m.IsClear);
    }

    private static bool Cleared(DarkUserMemoryDatabase userDb, EntityMQuest quest, int missionId)
        => userDb.EntityIUserQuestMission.Single(m => m.QuestId == quest.QuestId && m.QuestMissionId == missionId).IsClear;
}
