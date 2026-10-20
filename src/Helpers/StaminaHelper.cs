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
    /// Adds the stamina regenerated since the last update (one point per <paramref name="recoverySecond"/>
    /// seconds) up to <paramref name="maxStamina"/>. Stamina already at or above the max is left as it is.
    /// </summary>
    public static void Settle(EntityIUserStatus status, int maxStamina, int recoverySecond, long nowMs)
    {
        long maxMilliValue = maxStamina * 1000L;
        long elapsedMs = nowMs - status.StaminaUpdateDatetime;
        if (status.StaminaMilliValue >= maxMilliValue || elapsedMs <= 0 || recoverySecond <= 0)
        {
            return;
        }

        // One point (1000 milli) every recoverySecond seconds is one milli every recoverySecond ms.
        long regenerated = elapsedMs / recoverySecond;
        status.StaminaMilliValue = (int)Math.Min(status.StaminaMilliValue + regenerated, maxMilliValue);
        status.StaminaUpdateDatetime = nowMs;
    }

    /// <summary>
    /// <see cref="Settle(EntityIUserStatus, int, int, long)"/> with the max of the user's current level and the
    /// configured recovery time. Call it before any other change to stamina, so regeneration pending until now is kept.
    /// </summary>
    public static void Settle(EntityIUserStatus status, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, long nowMs)
        => Settle(status, LevelMaxStamina(masterDb, status.Level), gameConfig.UserStaminaRecoverySecond, nowMs);

    /// <summary>The regular stamina maximum of a user level; 0 when the level is unknown.</summary>
    public static int LevelMaxStamina(DarkMasterMemoryDatabase masterDb, int level)
        => masterDb.EntityMUserLevel.Find(level)?.MaxStamina ?? 0;

    /// <summary>Settles regeneration, then spends <paramref name="cost"/> stamina, never going below zero.</summary>
    public static void Consume(EntityIUserStatus status, int cost, int maxStamina, int recoverySecond, long nowMs)
    {
        Settle(status, maxStamina, recoverySecond, nowMs);
        status.StaminaMilliValue = Math.Max(status.StaminaMilliValue - cost * 1000, 0);
        status.StaminaUpdateDatetime = nowMs;
    }

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
