using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Important-item reward boosts: ownership, quest and item targeting, additive stacking with each other and campaigns.</summary>
public class ImportantItemEffectTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const int DailyRewardPlusItemId = 200020;
    private const long NowMs = 1_700_000_000_000;

    private static readonly QuestCampaignTarget DungeonQuest = new(500, QuestType.EVENT_QUEST, 30, EventQuestType.DUNGEON);
    private static readonly QuestCampaignTarget MainVeryHard = new(600, QuestType.MAIN_QUEST, 4, Difficulty: DifficultyType.VERY_HARD);

    [Fact]
    public void DailyRewardPlus_DoublesEveryBigHuntReward_InRealMasterData()
    {
        ImportantItemEffectHelper items = new(MasterDb);
        DarkUserMemoryDatabase userDb = UserOwning(DailyRewardPlusItemId);
        QuestCampaignTarget bigHunt = new QuestCampaignHelper(MasterDb).GetBigHuntQuestTarget(0);

        Assert.Equal(1000, items.DropBonusPermil(userDb, bigHunt, PossessionType.MATERIAL, 321007, NowMs));
        Assert.Equal(0, items.DropBonusPermil(UserOwning(), bigHunt, PossessionType.MATERIAL, 321007, NowMs));
        Assert.Equal(0, items.DropBonusPermil(userDb, DungeonQuest, PossessionType.MATERIAL, 321007, NowMs));
    }

    [Fact]
    public void PlusAndPlusPlusItems_StackAdditively()
    {
        ImportantItemEffectHelper items = Build(
            (1, 2, 500, [(ImportantItemEffectTargetQuestGroupType.EVENT_QUEST_TYPE, (int)EventQuestType.DUNGEON)], [(PossessionType.PARTS, 7)]),
            (2, 2, 1000, [(ImportantItemEffectTargetQuestGroupType.EVENT_QUEST_TYPE, (int)EventQuestType.DUNGEON)], [(PossessionType.PARTS, 7)]));

        Assert.Equal(1500, items.DropBonusPermil(UserOwning(1, 2), DungeonQuest, PossessionType.PARTS, 7, NowMs));
        Assert.Equal(500, items.DropBonusPermil(UserOwning(1), DungeonQuest, PossessionType.PARTS, 7, NowMs));
        Assert.Equal(0, items.DropBonusPermil(UserOwning(1, 2), DungeonQuest, PossessionType.PARTS, 8, NowMs));
    }

    [Fact]
    public void MainQuestDifficultyTarget_MatchesOnlyThatDifficulty()
    {
        ImportantItemEffectHelper items = Build(
            (1, 3, 500, [(ImportantItemEffectTargetQuestGroupType.MAIN_QUEST_DIFFICULTY_TYPE, (int)DifficultyType.VERY_HARD)], []));
        DarkUserMemoryDatabase userDb = UserOwning(1);

        Assert.Equal(500, items.DropBonusPermil(userDb, MainVeryHard, PossessionType.MATERIAL, 1, NowMs));
        Assert.Equal(0, items.DropBonusPermil(userDb, MainVeryHard with { Difficulty = DifficultyType.HARD }, PossessionType.MATERIAL, 1, NowMs));
    }

    [Fact]
    public void ItemBoostsAddToDropRateCampaigns()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMQuestCampaign.Add(new EntityMQuestCampaign
        {
            QuestCampaignId = 1,
            QuestCampaignTargetGroupId = 1,
            QuestCampaignEffectGroupId = 1,
            StartDatetime = 0,
            EndDatetime = long.MaxValue,
            TargetUserStatusType = TargetUserStatusType.ALL
        });
        masterDb.EntityMQuestCampaignTargetGroup.Add(new EntityMQuestCampaignTargetGroup { QuestCampaignTargetGroupId = 1, QuestCampaignTargetType = QuestCampaignTargetType.WHOLE_QUEST });
        masterDb.EntityMQuestCampaignEffectGroup.Add(new EntityMQuestCampaignEffectGroup
        {
            QuestCampaignEffectGroupId = 1,
            QuestCampaignEffectType = QuestCampaignEffectType.DROP_RATE,
            QuestCampaignEffectValue = 500
        });
        QuestCampaignHelper campaigns = new(masterDb);

        List<QuestReward> rewards = campaigns.ApplyDropCampaignBonuses(
            [new QuestReward { PossessionType = (int)PossessionType.MATERIAL, PossessionId = 1, Count = 10 }],
            DungeonQuest,
            NowMs,
            _ => 1000);

        Assert.Equal(25, rewards.Single().Count);
    }

    private static DarkUserMemoryDatabase UserOwning(params int[] importantItemIds)
    {
        DarkUserMemoryDatabase userDb = new() { UserId = 1 };
        foreach (int id in importantItemIds)
        {
            userDb.EntityIUserImportantItem.Add(new EntityIUserImportantItem { UserId = 1, ImportantItemId = id, Count = 1 });
        }

        return userDb;
    }

    /// <summary>One important item per row: (item id, effect type 2 = rate / 3 = count, permil, quest targets, item targets; none = every item).</summary>
    private static ImportantItemEffectHelper Build(
        params (int ItemId, int EffectType, int Permil, (ImportantItemEffectTargetQuestGroupType Type, int Value)[] Quests, (PossessionType Type, int Id)[] Items)[] rows)
    {
        DarkMasterMemoryDatabase masterDb = new();
        foreach ((int itemId, int effectType, int permil, (ImportantItemEffectTargetQuestGroupType, int)[] quests, (PossessionType, int)[] items) in rows)
        {
            masterDb.EntityMImportantItem.Add(new EntityMImportantItem { ImportantItemId = itemId, ImportantItemEffectId = itemId });
            masterDb.EntityMImportantItemEffect.Add(new EntityMImportantItemEffect
            {
                ImportantItemEffectId = itemId,
                ImportantItemEffectType = effectType,
                ImportantItemEffectTargetId = itemId,
                StartDatetime = 0,
                EndDatetime = long.MaxValue
            });
            int itemGroupId = items.Length == 0 ? 0 : itemId;
            if (effectType == 3)
            {
                masterDb.EntityMImportantItemEffectDropCount.Add(new EntityMImportantItemEffectDropCount
                {
                    ImportantItemEffectDropCountId = itemId,
                    CountPermil = permil,
                    ImportantItemEffectTargetQuestGroupId = itemId,
                    ImportantItemEffectTargetItemGroupId = itemGroupId
                });
            }
            else
            {
                masterDb.EntityMImportantItemEffectDropRate.Add(new EntityMImportantItemEffectDropRate
                {
                    ImportantItemEffectDropRateId = itemId,
                    RatePermil = permil,
                    ImportantItemEffectTargetQuestGroupId = itemId,
                    ImportantItemEffectTargetItemGroupId = itemGroupId
                });
            }

            foreach ((ImportantItemEffectTargetQuestGroupType type, int value) in quests)
            {
                masterDb.EntityMImportantItemEffectTargetQuestGroup.Add(new EntityMImportantItemEffectTargetQuestGroup
                {
                    ImportantItemEffectTargetQuestGroupId = itemId,
                    ImportantItemEffectTargetQuestGroupType = type,
                    TargetValue = value
                });
            }

            foreach ((PossessionType type, int id) in items)
            {
                masterDb.EntityMImportantItemEffectTargetItemGroup.Add(new EntityMImportantItemEffectTargetItemGroup
                {
                    ImportantItemEffectTargetItemGroupId = itemId,
                    PossessionType = type,
                    PossessionId = id
                });
            }
        }

        return new ImportantItemEffectHelper(masterDb);
    }
}
