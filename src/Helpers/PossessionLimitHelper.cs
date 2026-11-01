using MariesWonderland.Data;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// The possession count limits from master config (<c>POSSESSION_COUNT_LIMIT_*</c>): whether granting something would
/// take the player past what they can hold. A limit of 0 (not in the config) means no limit.
/// </summary>
public static class PossessionLimitHelper
{
    /// <summary>
    /// True when granting <paramref name="count"/> of a possession would go past its limit. Weapons and memoirs count
    /// the rows owned, one new row per grant. Convertible pieces never go past it, they convert first.
    /// </summary>
    public static bool WouldExceed(DarkUserMemoryDatabase userDb, GameConfig config, PossessionType type, int id, int count)
    {
        switch (type)
        {
            case PossessionType.MATERIAL:
                if (PieceConversionHelper.FindRule(id) != null)
                {
                    return false;
                }
                return Exceeds(userDb.EntityIUserMaterial.FirstOrDefault(m => m.MaterialId == id)?.Count ?? 0, count, config.MaterialCountLimit);
            case PossessionType.CONSUMABLE_ITEM:
                return Exceeds(userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == id)?.Count ?? 0, count, ConsumableLimit(config, id));
            case PossessionType.IMPORTANT_ITEM:
                return Exceeds(userDb.EntityIUserImportantItem.FirstOrDefault(i => i.ImportantItemId == id)?.Count ?? 0, count, config.ImportantItemCountLimit);
            case PossessionType.WEAPON:
            case PossessionType.WEAPON_ENHANCED:
                return Exceeds(userDb.EntityIUserWeapon.Count, 1, config.WeaponCountLimit);
            case PossessionType.PARTS:
            case PossessionType.PARTS_ENHANCED:
                return Exceeds(userDb.EntityIUserParts.Count, 1, config.PartsCountLimit);
            default:
                return false;
        }
    }

    private static int ConsumableLimit(GameConfig config, int consumableItemId)
    {
        if (consumableItemId == config.ConsumableItemIdForGold)
        {
            return config.MoneyCountLimit;
        }

        if (consumableItemId == config.ConsumableItemIdForMedal || consumableItemId == config.ConsumableItemIdForRareMedal)
        {
            return config.GachaMedalCountLimit;
        }

        return config.ConsumableItemCountLimit;
    }

    private static bool Exceeds(long owned, int adding, int limit) => limit > 0 && owned + adding > limit;
}
