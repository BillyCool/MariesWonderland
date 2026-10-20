using MariesWonderland.Data;
using MariesWonderland.Models.Entities;

namespace MariesWonderland.Helpers;

/// <summary>
/// Centralises stamina mutation logic. All code that adds or modifies stamina pools should go through here.
/// PVP stamina is a separate pool and will be added here when implemented.
/// </summary>
public static class StaminaHelper
{
    /// <summary>
    /// Adds <paramref name="amount"/> stamina to the user's regular stamina pool,
    /// capped at <paramref name="staminaMaxCount"/>. Updates the stamina timestamp.
    /// </summary>
    public static void AddStamina(EntityIUserStatus status, int amount, int staminaMaxCount, long nowMs)
    {
        int capMilliValue = Math.Max(staminaMaxCount, 0) * 1000;
        status.StaminaMilliValue = Math.Min(status.StaminaMilliValue + amount * 1000, capMilliValue);
        status.StaminaUpdateDatetime = nowMs;
    }

    /// <summary>
    /// Adds <paramref name="amount"/> stamina to the user's regular stamina pool,
    /// capped at <see cref="GameConfig.StaminaMaxCount"/>. Updates the stamina timestamp.
    /// </summary>
    /// <param name="status">The user status entity to mutate.</param>
    /// <param name="amount">Amount in regular stamina units (not milli).</param>
    /// <param name="gameConfig">Config providing the stamina cap.</param>
    /// <param name="nowMs">Current Unix timestamp in milliseconds for the update datetime.</param>
    public static void AddStamina(EntityIUserStatus status, int amount, GameConfig gameConfig, long nowMs)
        => AddStamina(status, amount, gameConfig.StaminaMaxCount, nowMs);

    /// <summary>
    /// Raises regular stamina to <paramref name="maxStamina"/> (the level's maximum), as on a level-up.
    /// Stamina already above it is kept.
    /// </summary>
    public static void RefillToMax(EntityIUserStatus status, int maxStamina, long nowMs)
    {
        status.StaminaMilliValue = Math.Max(status.StaminaMilliValue, maxStamina * 1000);
        status.StaminaUpdateDatetime = nowMs;
    }
}
