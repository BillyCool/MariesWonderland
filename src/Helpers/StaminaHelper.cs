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
    /// capped at <see cref="GameConfig.StaminaMaxCount"/>. Updates the stamina timestamp.
    /// </summary>
    /// <param name="status">The user status entity to mutate.</param>
    /// <param name="amount">Amount in regular stamina units (not milli).</param>
    /// <param name="gameConfig">Config providing the stamina cap.</param>
    /// <param name="nowMs">Current Unix timestamp in milliseconds for the update datetime.</param>
    public static void AddStamina(EntityIUserStatus status, int amount, GameConfig gameConfig, long nowMs)
    {
        int capMilliValue = gameConfig.StaminaMaxCount * 1000;
        status.StaminaMilliValue = Math.Min(status.StaminaMilliValue + amount * 1000, capMilliValue);
        status.StaminaUpdateDatetime = nowMs;
    }
}
