using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// The <c>rewardEffectId</c> of a quest reward: the reveal the result screen plays for it. 0 plays nothing, 1 and 2 a
/// normal reveal, 3 the high-rarity one. Same rule as the client uses for Big Hunt results: by the item's rarity, and
/// free gems and important items, which have none, get the high-rarity reveal.
/// </summary>
public static class RewardEffectHelper
{
    public const int Normal = 1;
    public const int SRare = 2;
    public const int Legend = 3;

    public static int EffectId(DarkMasterMemoryDatabase masterDb, PossessionType possessionType, int possessionId)
        => Rarity(masterDb, possessionType, possessionId) switch
        {
            RarityType.NORMAL or RarityType.RARE => Normal,
            RarityType.S_RARE => SRare,
            RarityType.SS_RARE or RarityType.LEGEND => Legend,
            _ => possessionType is PossessionType.FREE_GEM or PossessionType.IMPORTANT_ITEM ? Legend : Normal
        };

    /// <summary>The item's rarity, for the possession types that have one (UNKNOWN otherwise).</summary>
    private static RarityType Rarity(DarkMasterMemoryDatabase masterDb, PossessionType possessionType, int possessionId) => possessionType switch
    {
        PossessionType.COSTUME => CostumeRarity(masterDb, possessionId),
        PossessionType.COSTUME_ENHANCED => CostumeRarity(masterDb,
            masterDb.EntityMCostumeEnhanced.FirstOrDefault(c => c.CostumeEnhancedId == possessionId)?.CostumeId ?? 0),
        PossessionType.WEAPON => WeaponRarity(masterDb, possessionId),
        PossessionType.WEAPON_ENHANCED => WeaponRarity(masterDb,
            masterDb.EntityMWeaponEnhanced.FirstOrDefault(w => w.WeaponEnhancedId == possessionId)?.WeaponId ?? 0),
        PossessionType.PARTS => PartsRarity(masterDb, possessionId),
        PossessionType.PARTS_ENHANCED => PartsRarity(masterDb,
            masterDb.EntityMPartsEnhanced.FirstOrDefault(p => p.PartsEnhancedId == possessionId)?.PartsId ?? 0),
        PossessionType.MATERIAL => masterDb.EntityMMaterial.FirstOrDefault(m => m.MaterialId == possessionId)?.RarityType ?? RarityType.UNKNOWN,
        _ => RarityType.UNKNOWN
    };

    private static RarityType CostumeRarity(DarkMasterMemoryDatabase masterDb, int costumeId)
        => masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == costumeId)?.RarityType ?? RarityType.UNKNOWN;

    private static RarityType WeaponRarity(DarkMasterMemoryDatabase masterDb, int weaponId)
        => masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponId == weaponId)?.RarityType ?? RarityType.UNKNOWN;

    private static RarityType PartsRarity(DarkMasterMemoryDatabase masterDb, int partsId)
        => masterDb.EntityMParts.FirstOrDefault(p => p.PartsId == partsId)?.RarityType ?? RarityType.UNKNOWN;
}
