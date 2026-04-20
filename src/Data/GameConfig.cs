using MariesWonderland.Models.Entities;

namespace MariesWonderland.Data;

/// <summary>
/// Game-wide constants loaded from <see cref="EntityMConfig"/> master data at startup.
/// </summary>
public class GameConfig
{
    public int ConsumableItemIdForGold { get; init; }
    public int ConsumableItemIdForMedal { get; init; }
    public int ConsumableItemIdForRareMedal { get; init; }
    public int ConsumableItemIdForArenaCoin { get; init; }
    public int ConsumableItemIdForExploreTicket { get; init; }
    public int ConsumableItemIdForMomPoint { get; init; }
    public int ConsumableItemIdForPremiumGachaTicket { get; init; }
    public int ConsumableItemIdForQuestSkipTicket { get; init; }

    public int CharacterRebirthAvailableCount { get; init; }
    public int CharacterRebirthConsumeGold { get; init; }

    public int CostumeAwakenAvailableCount { get; init; }
    public int CostumeLimitBreakAvailableCount { get; init; }

    public int MaterialSameWeaponExpCoefficientPermil { get; init; }

    public int UserStaminaRecoverySecond { get; init; }
    public int RewardGachaDailyMaxCount { get; init; }
    public int QuestSkipMaxCountAtOnce { get; init; }

    public int WeaponLimitBreakAvailableCount { get; init; }

    /// <summary>Builds a <see cref="GameConfig"/> from the loaded master config rows.</summary>
    public static GameConfig From(IEnumerable<EntityMConfig> configs)
    {
        Dictionary<string, string> kv = configs.ToDictionary(c => c.ConfigKey, c => c.Value);

        return new GameConfig
        {
            ConsumableItemIdForGold = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_GOLD"),
            ConsumableItemIdForMedal = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_MEDAL"),
            ConsumableItemIdForRareMedal = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_RARE_MEDAL"),
            ConsumableItemIdForArenaCoin = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_ARENA_COIN"),
            ConsumableItemIdForExploreTicket = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_EXPLORE_TICKET"),
            ConsumableItemIdForMomPoint = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_MOM_POINT"),
            ConsumableItemIdForPremiumGachaTicket = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_PREMIUM_GACHA_TICKET"),
            ConsumableItemIdForQuestSkipTicket = ParseInt(kv, "CONSUMABLE_ITEM_ID_FOR_QUEST_SKIP_TICKET"),

            CharacterRebirthAvailableCount = ParseInt(kv, "CHARACTER_REBIRTH_AVAILABLE_COUNT"),
            CharacterRebirthConsumeGold = ParseInt(kv, "CHARACTER_REBIRTH_CONSUME_GOLD"),

            CostumeAwakenAvailableCount = ParseInt(kv, "COSTUME_AWAKEN_AVAILABLE_COUNT"),
            CostumeLimitBreakAvailableCount = ParseInt(kv, "COSTUME_LIMIT_BREAK_AVAILABLE_COUNT"),

            MaterialSameWeaponExpCoefficientPermil = ParseInt(kv, "MATERIAL_SAME_WEAPON_EXP_COEFFICIENT_PERMIL"),

            UserStaminaRecoverySecond = ParseInt(kv, "USER_STAMINA_RECOVERY_SECOND"),
            RewardGachaDailyMaxCount = ParseInt(kv, "REWARD_GACHA_DAILY_MAX_COUNT"),
            QuestSkipMaxCountAtOnce = ParseInt(kv, "QUEST_SKIP_MAX_COUNT_AT_ONCE"),

            WeaponLimitBreakAvailableCount = ParseInt(kv, "WEAPON_LIMIT_BREAK_AVAILABLE_COUNT"),
        };
    }

    private static int ParseInt(Dictionary<string, string> kv, string key)
        => kv.TryGetValue(key, out string? s) && int.TryParse(s, out int v) ? v : 0;
}
