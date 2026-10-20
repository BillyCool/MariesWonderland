using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Data;

public class TableExtensionsTests(MasterDatabaseFixture fixture)
{
    [Fact]
    public void Find_SingleKey_ReturnsMatchingRow()
    {
        List<EntityMQuestScene> table =
        [
            new EntityMQuestScene { QuestSceneId = 1, QuestId = 10 },
            new EntityMQuestScene { QuestSceneId = 2, QuestId = 20 }
        ];

        Assert.Equal(20, table.Find(2)?.QuestId);
        Assert.Null(table.Find(3));
    }

    [Fact]
    public void Find_CompositeKey_ReturnsMatchingRow()
    {
        List<EntityIUserCostume> table =
        [
            new EntityIUserCostume { UserId = 1, UserCostumeUuid = "a", CostumeId = 100 },
            new EntityIUserCostume { UserId = 1, UserCostumeUuid = "b", CostumeId = 200 }
        ];

        Assert.Equal(200, table.Find(1L, "b")?.CostumeId);
        Assert.Null(table.Find(2L, "b"));
    }

    [Fact]
    public void Find_ConvertsValuesToKeyPropertyTypes()
    {
        List<EntityIUserDeck> table =
        [
            new EntityIUserDeck { UserId = 1, DeckType = DeckType.QUEST, UserDeckNumber = 3 }
        ];

        // int literal for the long UserId, enum passed as its type
        Assert.NotNull(table.Find(1, DeckType.QUEST, 3));
        Assert.Null(table.Find(1, DeckType.QUEST, 4));
    }

    [Fact]
    public void Find_WrongNumberOfKeyValues_Throws()
    {
        List<EntityIUserCostume> table = [];

        Assert.Throws<ArgumentException>(() => table.Find(1L));
    }

    [Fact]
    public void Find_ReadOnlyMasterTable_UsesIndexAndKeepsFirstDuplicate()
    {
        DarkMasterMemoryDatabase masterDb = new()
        {
            EntityMCostumeLotteryEffectOddsGroup =
            [
                new EntityMCostumeLotteryEffectOddsGroup { CostumeLotteryEffectOddsGroupId = 1, OddsNumber = 1, Weight = 10 },
                new EntityMCostumeLotteryEffectOddsGroup { CostumeLotteryEffectOddsGroupId = 1, OddsNumber = 1, Weight = 99 },
                new EntityMCostumeLotteryEffectOddsGroup { CostumeLotteryEffectOddsGroupId = 1, OddsNumber = 2, Weight = 20 }
            ]
        };

        TableExtensions.MarkReadOnly(masterDb);

        Assert.Equal(10, masterDb.EntityMCostumeLotteryEffectOddsGroup.Find(1, 1)?.Weight);
        Assert.Equal(20, masterDb.EntityMCostumeLotteryEffectOddsGroup.Find(1, 2)?.Weight);
        Assert.Null(masterDb.EntityMCostumeLotteryEffectOddsGroup.Find(2, 1));
    }

    [Fact]
    public void EveryClientTable_HasResolvablePrimaryKey()
    {
        Type[] rowTypes =
        [
            .. typeof(DarkMasterMemoryDatabase).GetProperties().Select(p => p.PropertyType),
            .. typeof(DarkUserMemoryDatabase).GetProperties().Select(p => p.PropertyType)
        ];

        List<string> missing = [];
        foreach (Type listType in rowTypes.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)))
        {
            Type rowType = listType.GetGenericArguments()[0];
            if (rowType.Name.StartsWith("EntityS"))
                continue; // server-only tables get keys with persistence

            try
            {
                _ = typeof(EntityKey<>).MakeGenericType(rowType).GetProperty(nameof(EntityKey<object>.Properties))!.GetValue(null);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                missing.Add(rowType.Name);
            }
        }

        Assert.Empty(missing);
    }

    [Fact]
    public void Find_OnLoadedMasterData_MatchesLinearScan()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        DarkMasterMemoryDatabase masterDb = fixture.MasterDb;

        foreach (EntityMQuestScene scene in masterDb.EntityMQuestScene.Take(500))
        {
            EntityMQuestScene expected = masterDb.EntityMQuestScene.First(s => s.QuestSceneId == scene.QuestSceneId);
            Assert.Same(expected, masterDb.EntityMQuestScene.Find(scene.QuestSceneId));
        }
    }
}
