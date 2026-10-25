using MariesWonderland.Data;
using MariesWonderland.Models.Entities;

namespace MariesWonderland.Helpers;

/// <summary>Level lookups from EXP thresholds in <see cref="EntityMNumericalParameterMap"/>.</summary>
public static class LevelHelper
{
    /// <summary>Numerical parameter map holding the user level EXP thresholds.</summary>
    public const int UserExpMapId = 1;

    /// <summary>Numerical parameter map holding the character level EXP thresholds.</summary>
    public const int CharacterExpMapId = 31;

    /// <summary>
    /// Returns the highest level whose EXP threshold in the given map is at or below <paramref name="exp"/>,
    /// or 1 when none is.
    /// </summary>
    public static int LevelForExp(DarkMasterMemoryDatabase masterDb, int numericalParameterMapId, int exp)
    {
        int level = 1;
        foreach (EntityMNumericalParameterMap row in masterDb.EntityMNumericalParameterMap
            .Where(r => r.NumericalParameterMapId == numericalParameterMapId)
            .OrderBy(r => r.ParameterKey))
        {
            if (exp < row.ParameterValue)
            {
                break;
            }

            level = row.ParameterKey;
        }

        return level;
    }

    /// <summary>
    /// Returns the level for <paramref name="exp"/> and the EXP to store, which stops at the max level's
    /// threshold so EXP doesn't pile up past the cap.
    /// </summary>
    public static (int Level, int Exp) ApplyExp(DarkMasterMemoryDatabase masterDb, int numericalParameterMapId, int exp)
        => ApplyExp(masterDb, numericalParameterMapId, exp, 0);

    /// <summary>
    /// Like <see cref="ApplyExp(DarkMasterMemoryDatabase, int, int)"/>, but stops at <paramref name="maxLevel"/>
    /// (the instance's own cap) when it is below the table's last level. 0 means the table's last level.
    /// </summary>
    public static (int Level, int Exp) ApplyExp(DarkMasterMemoryDatabase masterDb, int numericalParameterMapId, int exp, int maxLevel)
    {
        List<EntityMNumericalParameterMap> thresholds = [.. masterDb.EntityMNumericalParameterMap
            .Where(r => r.NumericalParameterMapId == numericalParameterMapId)];
        int maxLevelExp = thresholds
            .Where(r => maxLevel <= 0 || r.ParameterKey <= maxLevel)
            .Select(r => r.ParameterValue)
            .DefaultIfEmpty(int.MaxValue)
            .Max();
        int cappedExp = Math.Min(exp, maxLevelExp);
        return (LevelForExp(masterDb, numericalParameterMapId, cappedExp), cappedExp);
    }

    /// <summary>
    /// A costume's max level: its rarity's max-level function at its limit-break count, plus the costume
    /// level-limit-up of every rebirth its character has been through.
    /// </summary>
    public static int CostumeMaxLevel(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, EntityIUserCostume costume, EntityMCostume costumeMaster)
    {
        EntityMCostumeRarity? rarity = masterDb.EntityMCostumeRarity.FirstOrDefault(r => r.RarityType == costumeMaster.RarityType);
        if (rarity == null)
        {
            return 0;
        }

        int maxLevel = NumericalFunctionHelper.Evaluate(masterDb, rarity.MaxLevelNumericalFunctionId, costume.LimitBreakCount);

        int rebirthCount = userDb.EntityIUserCharacterRebirth.FirstOrDefault(r => r.CharacterId == costumeMaster.CharacterId)?.RebirthCount ?? 0;
        EntityMCharacterRebirth? rebirth = masterDb.EntityMCharacterRebirth.FirstOrDefault(r => r.CharacterId == costumeMaster.CharacterId);
        if (rebirth != null && rebirthCount > 0)
        {
            maxLevel += masterDb.EntityMCharacterRebirthStepGroup
                .Where(s => s.CharacterRebirthStepGroupId == rebirth.CharacterRebirthStepGroupId && s.BeforeRebirthCount < rebirthCount)
                .Sum(s => s.CostumeLevelLimitUp);
        }

        return maxLevel;
    }

    /// <summary>
    /// A weapon's base max level: its enhance max-level function with no limit breaks or awakening.
    /// </summary>
    public static int WeaponBaseMaxLevel(DarkMasterMemoryDatabase masterDb, EntityMWeapon weaponMaster)
    {
        EntityMWeaponSpecificEnhance? enhance = WeaponHelper.ResolveEnhance(masterDb, weaponMaster);
        return enhance == null ? 0 : NumericalFunctionHelper.Evaluate(masterDb, enhance.MaxLevelNumericalFunctionId, 0);
    }

    /// <summary>
    /// A weapon's max level: its enhance max-level function at its limit-break count, plus the awaken
    /// level-limit-up once the weapon is awakened.
    /// </summary>
    public static int WeaponMaxLevel(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, EntityIUserWeapon weapon, EntityMWeapon weaponMaster)
    {
        EntityMWeaponSpecificEnhance? enhance = WeaponHelper.ResolveEnhance(masterDb, weaponMaster);
        if (enhance == null)
        {
            return 0;
        }

        int maxLevel = NumericalFunctionHelper.Evaluate(masterDb, enhance.MaxLevelNumericalFunctionId, weapon.LimitBreakCount);
        if (userDb.EntityIUserWeaponAwaken.Any(a => a.UserWeaponUuid == weapon.UserWeaponUuid))
        {
            maxLevel += masterDb.EntityMWeaponAwaken.FirstOrDefault(a => a.WeaponId == weapon.WeaponId)?.LevelLimitUp ?? 0;
        }

        return maxLevel;
    }
}
