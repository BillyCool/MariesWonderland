using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Tests.Helpers;

/// <summary>The result-screen reveal of a reward: by rarity, with free gems and important items high-rarity.</summary>
public class RewardEffectHelperTests
{
    [Fact]
    public void EffectId_FollowsTheItemsRarity_IncludingEnhancedVariants()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMMaterial.Add(new EntityMMaterial { MaterialId = 1, RarityType = RarityType.RARE });
        masterDb.EntityMWeapon.Add(new EntityMWeapon { WeaponId = 2, RarityType = RarityType.S_RARE });
        masterDb.EntityMCostume.Add(new EntityMCostume { CostumeId = 3, RarityType = RarityType.LEGEND });
        masterDb.EntityMParts.Add(new EntityMParts { PartsId = 4, RarityType = RarityType.SS_RARE });
        masterDb.EntityMWeaponEnhanced.Add(new EntityMWeaponEnhanced { WeaponEnhancedId = 20, WeaponId = 2 });

        Assert.Equal(RewardEffectHelper.Normal, RewardEffectHelper.EffectId(masterDb, PossessionType.MATERIAL, 1));
        Assert.Equal(RewardEffectHelper.SRare, RewardEffectHelper.EffectId(masterDb, PossessionType.WEAPON, 2));
        Assert.Equal(RewardEffectHelper.SRare, RewardEffectHelper.EffectId(masterDb, PossessionType.WEAPON_ENHANCED, 20));
        Assert.Equal(RewardEffectHelper.Legend, RewardEffectHelper.EffectId(masterDb, PossessionType.COSTUME, 3));
        Assert.Equal(RewardEffectHelper.Legend, RewardEffectHelper.EffectId(masterDb, PossessionType.PARTS, 4));
    }

    [Fact]
    public void EffectId_WithoutRarity_IsNormal_ExceptFreeGemsAndImportantItems()
    {
        DarkMasterMemoryDatabase masterDb = new();

        Assert.Equal(RewardEffectHelper.Normal, RewardEffectHelper.EffectId(masterDb, PossessionType.CONSUMABLE_ITEM, 1));
        Assert.Equal(RewardEffectHelper.Normal, RewardEffectHelper.EffectId(masterDb, PossessionType.COMPANION, 1));
        Assert.Equal(RewardEffectHelper.Legend, RewardEffectHelper.EffectId(masterDb, PossessionType.FREE_GEM, 0));
        Assert.Equal(RewardEffectHelper.Legend, RewardEffectHelper.EffectId(masterDb, PossessionType.IMPORTANT_ITEM, 5));
    }
}
