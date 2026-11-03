using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Helpers;

/// <summary>The memoir tables (Data/Custom/MemoirSubStatusRolls.json, MemoirDrops.json) and how memoirs use them.</summary>
public class MemoirTableTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private static MemoirTable Table => MemoirTable.Load(MemoirTable.RollsPath, MemoirTable.DropsPath);

    [Theory]
    [InlineData(RarityType.SS_RARE, new[] { 4, 5 })]
    [InlineData(RarityType.S_RARE, new[] { 3, 4, 5 })]
    [InlineData(RarityType.RARE, new[] { 3, 4, 5 })]
    [InlineData(RarityType.NORMAL, new[] { 3 })]
    public void RollGrade_OnlyGivesTheGradesOfTheRarity(RarityType rarity, int[] grades)
    {
        Random random = new(1);
        HashSet<int> seen = [.. Enumerable.Range(0, 2000).Select(_ => Table.RollGrade(rarity, random))];

        Assert.Equal(grades.Order(), seen.Order());
    }

    [Fact]
    public void RollGrade_FollowsTheOdds()
    {
        Random random = new(7);
        int[] grades = [.. Enumerable.Range(0, 20000).Select(_ => Table.RollGrade(RarityType.S_RARE, random))];

        Assert.InRange(grades.Count(g => g == 3) / 20000.0, 0.28, 0.32);
        Assert.InRange(grades.Count(g => g == 4) / 20000.0, 0.48, 0.52);
        Assert.InRange(grades.Count(g => g == 5) / 20000.0, 0.18, 0.22);
    }

    [Theory]
    [InlineData(5, 4, 4)]
    [InlineData(4, 3, 4)]
    [InlineData(3, 2, 3)]
    [InlineData(2, 1, 2)]
    [InlineData(1, 0, 1)]
    public void RollStartingSlots_StaysWithinTheGradesCounts_AndBothCountsCome(int grade, int fewest, int most)
    {
        Random random = new(3);
        HashSet<int> seen = [.. Enumerable.Range(0, 500).Select(_ => Table.RollStartingSlots(grade, random))];

        Assert.Equal(Enumerable.Range(fewest, most - fewest + 1), seen.Order());
    }

    [Fact]
    public void Rolls_StayInRangeAndStep_ForEveryRarity()
    {
        Random random = new(5);
        foreach (RarityType rarity in new[] { RarityType.SS_RARE, RarityType.S_RARE, RarityType.RARE, RarityType.NORMAL })
        {
            for (int i = 0; i < 300; i++)
            {
                MemoirSubStatusRoll roll = Table.PickNewRoll(rarity, [], random)!;
                int value = roll.RollValue(random);
                Assert.InRange(value, roll.MinValue, roll.MaxValue);
                Assert.Equal(0, (value - roll.MinValue) % roll.Step);
            }
        }
    }

    [Fact]
    public void PickNewRoll_NeverRepeatsAKind_AndKnowsWhenNoneIsLeft()
    {
        Random random = new(9);
        List<StatusKindType> used = [];
        for (int i = 0; i < 6; i++)
        {
            MemoirSubStatusRoll roll = Table.PickNewRoll(RarityType.SS_RARE, used, random)!;
            Assert.DoesNotContain(roll.StatusKindType, used);
            used.Add(roll.StatusKindType);
        }

        Assert.Null(Table.PickNewRoll(RarityType.SS_RARE, used, random));
    }

    [Fact]
    public void RainbowRolls_MatchTheRealSavesLotteryIds()
    {
        MemoirTable table = Table;

        Assert.Equal((StatusKindType.CRITICAL_RATIO, 30, 50), Describe(table.FindRoll(RarityType.SS_RARE, 34)!));
        Assert.Equal((StatusKindType.HP, 650, 725), Describe(table.FindRoll(RarityType.SS_RARE, 30)!));
        Assert.Equal((StatusKindType.VITALITY, 15, 25), Describe(table.FindRoll(RarityType.SS_RARE, 32)!));
        Assert.Equal((StatusKindType.CRITICAL_ATTACK, 30, 40), Describe(table.FindRoll(RarityType.S_RARE, 26)!));
    }

    [Fact]
    public void AdvanceLevel_FromNothingToMax_FillsFourSlotsThenUpgradesOne()
    {
        MemoirTable table = Table;
        Random random = new(11);
        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserParts parts = new() { UserId = 1, UserPartsUuid = "p", PartsId = 1, Level = 1 };

        MemoirSubStatusHelper.AdvanceLevel(userDb, parts, RarityType.SS_RARE, 1, 2, table, random);
        Assert.Empty(userDb.EntityIUserPartsStatusSub);

        for (int level = 3; level <= 15; level++)
        {
            MemoirSubStatusHelper.AdvanceLevel(userDb, parts, RarityType.SS_RARE, level - 1, level, table, random);
        }

        Assert.Equal([1, 2, 3, 4], userDb.EntityIUserPartsStatusSub.Select(s => s.StatusIndex).Order());
        Assert.Equal(5, userDb.EntityIUserPartsStatusSub.Sum(s => s.Level));
        Assert.Equal(4, userDb.EntityIUserPartsStatusSub.Select(s => s.StatusKindType).Distinct().Count());
    }

    [Fact]
    public void AdvanceLevel_SeveralLevelsAtOnce_CountsEveryUpgradeLevelPassed()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserParts parts = new() { UserId = 1, UserPartsUuid = "p", PartsId = 1, Level = 1 };

        MemoirSubStatusHelper.AdvanceLevel(userDb, parts, RarityType.SS_RARE, 1, 10, Table, new Random(2));

        Assert.Equal(3, userDb.EntityIUserPartsStatusSub.Count);
    }

    [Fact]
    public void RainbowDrop_RollsGradeFourOrFive_AndTheSlotsOfThatGrade()
    {
        EntityMParts rainbow = MasterDb.EntityMParts.Where(p => p.RarityType == RarityType.SS_RARE).OrderBy(p => p.PartsId).First();
        MemoirTable table = Table;
        Random random = new(13);
        HashSet<int> slotCounts = [];

        for (int i = 0; i < 200; i++)
        {
            DarkUserMemoryDatabase userDb = CreateUserDb();
            (int grantedId, bool sold, EntityIUserParts? granted) = PossessionHelper.GrantOrSellPartsDrop(
                userDb, rainbow.PartsId, MasterDb, 1, new HashSet<int>(), new HashSet<int>(), 1000, table, random);
            EntityMParts def = MasterDb.EntityMParts.Single(p => p.PartsId == grantedId);
            List<EntityIUserPartsStatusSub> subs = [.. userDb.EntityIUserPartsStatusSub];

            Assert.False(sold);
            Assert.Equal(rainbow.PartsGroupId, def.PartsGroupId);
            Assert.Contains(def.PartsInitialLotteryId, new[] { 4, 5 });
            Assert.InRange(subs.Count, def.PartsInitialLotteryId == 5 ? 4 : 3, 4);
            Assert.All(subs, s =>
            {
                Assert.Equal(granted!.UserPartsUuid, s.UserPartsUuid);
                Assert.Equal(1, s.Level);
                MemoirSubStatusRoll roll = table.FindRoll(RarityType.SS_RARE, s.PartsStatusSubLotteryId)!;
                Assert.InRange(s.StatusChangeValue, roll.MinValue, roll.MaxValue);
            });
            slotCounts.Add(subs.Count);
        }

        Assert.Equal([3, 4], slotCounts.Order());
    }

    private static (StatusKindType, int, int) Describe(MemoirSubStatusRoll roll) => (roll.StatusKindType, roll.MinValue, roll.MaxValue);
}
