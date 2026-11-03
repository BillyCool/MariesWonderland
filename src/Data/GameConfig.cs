using MariesWonderland.Models.Entities;

namespace MariesWonderland.Data;

/// <summary>
/// Game-wide constants loaded from <see cref="EntityMConfig"/> master data at startup.
/// </summary>
public class GameConfig
{
    /// <summary>How long a gift stays in the gift box, for every gift source (confirmed 30 days).</summary>
    public const int GiftExpirationDays = 30;

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
    public int CostumeLotteryEffectUnlockSlotConsumeGold { get; init; }
    public int CostumeLotteryEffectDrawSlotConsumeGold { get; init; }

    public int MaterialSameWeaponExpCoefficientPermil { get; init; }

    /// <summary>Share of a fodder weapon's own exp that enhancing with it passes on (permil).</summary>
    public int WeaponEnhanceCalcCoefficientPermil { get; init; }

    public int UserStaminaRecoverySecond { get; init; }
    public int StaminaMaxCount { get; init; }
    public int RewardGachaDailyMaxCount { get; init; }
    public int QuestSkipMaxCountAtOnce { get; init; }

    public int WeaponLimitBreakAvailableCount { get; init; }

    /// <summary>How much of one kind a player can hold (rows for weapons and memoirs). Gold uses the money limit, the medals the gacha medal limit.</summary>
    public int MaterialCountLimit { get; init; }
    public int ConsumableItemCountLimit { get; init; }
    public int ImportantItemCountLimit { get; init; }
    public int WeaponCountLimit { get; init; }
    public int PartsCountLimit { get; init; }
    public int MoneyCountLimit { get; init; }
    public int GachaMedalCountLimit { get; init; }

    public int UserLevelExpNumericalParameterMapId { get; init; }

    public int UserFriendMaxNumber { get; init; }
    public int UserFriendReceiveRequestMaxNumber { get; init; }
    public int UserFriendSendCheerMaxNumber { get; init; }
    public int UserFriendReceiveCheerMaxNumber { get; init; }

    /// <summary>Arena battle points (BP): the regeneration cap, the cost of a battle and of a new opponent list.</summary>
    public int PvpMaxBattlePoint { get; init; }
    public int PvpBattleConsumeBattlePoint { get; init; }
    public int PvpUpdateMatchingConsumeBattlePoint { get; init; }
    public int UserBattlePointRecoverySecond { get; init; }

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
            CostumeLotteryEffectUnlockSlotConsumeGold = ParseInt(kv, "COSTUME_LOTTERY_EFFECT_UNLOCK_SLOT_CONSUME_GOLD"),
            CostumeLotteryEffectDrawSlotConsumeGold = ParseInt(kv, "COSTUME_LOTTERY_EFFECT_DRAW_SLOT_CONSUME_GOLD"),

            MaterialSameWeaponExpCoefficientPermil = ParseInt(kv, "MATERIAL_SAME_WEAPON_EXP_COEFFICIENT_PERMIL"),
            WeaponEnhanceCalcCoefficientPermil = ParseInt(kv, "WEAPON_ENHANCE_CALC_COEFFICIENT_PERMIL"),

            UserStaminaRecoverySecond = ParseInt(kv, "USER_STAMINA_RECOVERY_SECOND"),
            StaminaMaxCount = ParseInt(kv, "POSSESSION_COUNT_LIMIT_STAMINA"),
            RewardGachaDailyMaxCount = ParseInt(kv, "REWARD_GACHA_DAILY_MAX_COUNT"),
            QuestSkipMaxCountAtOnce = ParseInt(kv, "QUEST_SKIP_MAX_COUNT_AT_ONCE"),

            WeaponLimitBreakAvailableCount = ParseInt(kv, "WEAPON_LIMIT_BREAK_AVAILABLE_COUNT"),

            MaterialCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_MATERIAL"),
            ConsumableItemCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_CONSUMABLE_ITEM"),
            ImportantItemCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_IMPORTANT_ITEM"),
            WeaponCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_WEAPON"),
            PartsCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_PARTS"),
            MoneyCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_MONEY"),
            GachaMedalCountLimit = ParseInt(kv, "POSSESSION_COUNT_LIMIT_GACHA_MEDAL"),

            UserLevelExpNumericalParameterMapId = ParseInt(kv, "USER_LEVEL_EXP_NUMERICAL_PARAMETER_MAP_ID"),

            UserFriendMaxNumber = ParseInt(kv, "USER_FRIEND_MAX_NUMBER"),
            UserFriendReceiveRequestMaxNumber = ParseInt(kv, "USER_FRIEND_RECEIVE_REQUEST_MAX_NUMBER"),
            UserFriendSendCheerMaxNumber = ParseInt(kv, "USER_FRIEND_SEND_CHEER_MAX_NUMBER"),
            UserFriendReceiveCheerMaxNumber = ParseInt(kv, "USER_FRIEND_RECEIVE_CHEER_MAX_NUMBER"),

            PvpMaxBattlePoint = ParseInt(kv, "PVP_MAX_BATTLE_POINT"),
            PvpBattleConsumeBattlePoint = ParseInt(kv, "PVP_BATTLE_CONSUME_BATTLE_POINT"),
            PvpUpdateMatchingConsumeBattlePoint = ParseInt(kv, "PVP_UPDATE_MATCHING_CONSUME_BATTLE_POINT"),
            UserBattlePointRecoverySecond = ParseInt(kv, "USER_BATTLE_POINT_RECOVERY_SECOND"),
        };
    }

    private static int ParseInt(Dictionary<string, string> kv, string key)
        => kv.TryGetValue(key, out string? s) && int.TryParse(s, out int v) ? v : 0;
}
