using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Per-instance level caps and weapon enhance resolution against real master data.</summary>
public class LevelCapTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private static DarkUserMemoryDatabase UserDb() => new() { UserId = 1 };

    [Fact]
    public void ApplyExp_StopsAtTheGivenMaxLevel()
    {
        int mapId = LevelHelper.UserExpMapId;
        int level10Exp = MasterDb.EntityMNumericalParameterMap.Single(r => r.NumericalParameterMapId == mapId && r.ParameterKey == 10).ParameterValue;

        (int level, int exp) = LevelHelper.ApplyExp(MasterDb, mapId, int.MaxValue / 2, maxLevel: 10);

        Assert.Equal(10, level);
        Assert.Equal(level10Exp, exp);
    }

    [Fact]
    public void CostumeMaxLevel_RisesWithLimitBreakAndRebirth()
    {
        EntityMCharacterRebirth rebirth = MasterDb.EntityMCharacterRebirth.OrderBy(r => r.CharacterId).First();
        EntityMCostume costumeMaster = MasterDb.EntityMCostume.First(c => c.CharacterId == rebirth.CharacterId);
        DarkUserMemoryDatabase userDb = UserDb();
        EntityIUserCostume costume = new() { UserCostumeUuid = "c", CostumeId = costumeMaster.CostumeId };

        int baseCap = LevelHelper.CostumeMaxLevel(MasterDb, userDb, costume, costumeMaster);
        costume.LimitBreakCount = 4;
        int limitBrokenCap = LevelHelper.CostumeMaxLevel(MasterDb, userDb, costume, costumeMaster);
        userDb.EntityIUserCharacterRebirth.Add(new EntityIUserCharacterRebirth { UserId = 1, CharacterId = rebirth.CharacterId, RebirthCount = 1 });
        int rebornCap = LevelHelper.CostumeMaxLevel(MasterDb, userDb, costume, costumeMaster);

        int firstStepLimitUp = MasterDb.EntityMCharacterRebirthStepGroup
            .Single(s => s.CharacterRebirthStepGroupId == rebirth.CharacterRebirthStepGroupId && s.BeforeRebirthCount == 0)
            .CostumeLevelLimitUp;
        Assert.True(baseCap > 0);
        Assert.True(limitBrokenCap > baseCap);
        Assert.Equal(limitBrokenCap + firstStepLimitUp, rebornCap);
    }

    [Fact]
    public void WeaponMaxLevel_AwakenAddsItsLevelLimitUp()
    {
        EntityMWeaponAwaken awaken = MasterDb.EntityMWeaponAwaken.First(a => a.LevelLimitUp > 0);
        EntityMWeapon weaponMaster = MasterDb.EntityMWeapon.Single(w => w.WeaponId == awaken.WeaponId);
        DarkUserMemoryDatabase userDb = UserDb();
        EntityIUserWeapon weapon = new() { UserWeaponUuid = "w", WeaponId = weaponMaster.WeaponId, LimitBreakCount = 4 };

        int cap = LevelHelper.WeaponMaxLevel(MasterDb, userDb, weapon, weaponMaster);
        userDb.EntityIUserWeaponAwaken.Add(new EntityIUserWeaponAwaken { UserId = 1, UserWeaponUuid = "w" });
        int awakenedCap = LevelHelper.WeaponMaxLevel(MasterDb, userDb, weapon, weaponMaster);

        Assert.True(cap > 0);
        Assert.Equal(cap + awaken.LevelLimitUp, awakenedCap);
    }

    [Fact]
    public void ResolveEnhance_WithoutASpecificRow_UsesTheRarityRow()
    {
        EntityMWeapon weaponMaster = MasterDb.EntityMWeapon.First(w => w.WeaponSpecificEnhanceId == 0);
        EntityMWeaponRarity rarity = MasterDb.EntityMWeaponRarity.Single(r => r.RarityType == weaponMaster.RarityType);

        EntityMWeaponSpecificEnhance? enhance = WeaponHelper.ResolveEnhance(MasterDb, weaponMaster);

        Assert.NotNull(enhance);
        Assert.Equal(rarity.MaxLevelNumericalFunctionId, enhance.MaxLevelNumericalFunctionId);
        Assert.Equal(rarity.RequiredExpForLevelUpNumericalParameterMapId, enhance.RequiredExpForLevelUpNumericalParameterMapId);
    }

    [Fact]
    public void ResolveLevelingEnhance_UsesTheEvolutionChainRoot()
    {
        // An evolved weapon whose own EXP table differs from its chain root's.
        (EntityMWeapon evolved, EntityMWeapon root) = MasterDb.EntityMWeaponEvolutionGroup
            .Where(e => e.EvolutionOrder > 1)
            .Select(e => (
                Evolved: MasterDb.EntityMWeapon.Single(w => w.WeaponId == e.WeaponId),
                Root: MasterDb.EntityMWeapon.Single(w => w.WeaponId == MasterDb.EntityMWeaponEvolutionGroup
                    .Where(g => g.WeaponEvolutionGroupId == e.WeaponEvolutionGroupId)
                    .OrderBy(g => g.EvolutionOrder)
                    .First().WeaponId)))
            .First(p => WeaponHelper.ResolveEnhance(MasterDb, p.Evolved)!.RequiredExpForLevelUpNumericalParameterMapId
                != WeaponHelper.ResolveEnhance(MasterDb, p.Root)!.RequiredExpForLevelUpNumericalParameterMapId);

        EntityMWeaponSpecificEnhance? leveling = WeaponHelper.ResolveLevelingEnhance(MasterDb, evolved);

        Assert.Equal(WeaponHelper.ResolveEnhance(MasterDb, root)!.RequiredExpForLevelUpNumericalParameterMapId, leveling!.RequiredExpForLevelUpNumericalParameterMapId);
    }
}
