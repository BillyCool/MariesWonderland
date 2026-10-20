using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Creates all entities associated with granting a weapon: the weapon instance, note record,
/// ability/skill slots from master data, and story unlocks triggered by the ACQUISITION condition.
/// </summary>
public static class WeaponHelper
{
    /// <summary>
    /// The weapon's enhance parameters: its own <see cref="EntityMWeaponSpecificEnhance"/> row, or, for the
    /// many weapons without one (id 0), its rarity's row, which has the same columns.
    /// </summary>
    public static EntityMWeaponSpecificEnhance? ResolveEnhance(DarkMasterMemoryDatabase masterDb, EntityMWeapon weaponMaster)
    {
        if (weaponMaster.WeaponSpecificEnhanceId != 0)
        {
            return masterDb.EntityMWeaponSpecificEnhance.FirstOrDefault(e => e.WeaponSpecificEnhanceId == weaponMaster.WeaponSpecificEnhanceId);
        }

        EntityMWeaponRarity? rarity = masterDb.EntityMWeaponRarity.FirstOrDefault(r => r.RarityType == weaponMaster.RarityType);
        if (rarity == null)
        {
            return null;
        }

        return new EntityMWeaponSpecificEnhance
        {
            BaseEnhancementObtainedExp = rarity.BaseEnhancementObtainedExp,
            SellPriceNumericalFunctionId = rarity.SellPriceNumericalFunctionId,
            MaxLevelNumericalFunctionId = rarity.MaxLevelNumericalFunctionId,
            MaxSkillLevelNumericalFunctionId = rarity.MaxSkillLevelNumericalFunctionId,
            MaxAbilityLevelNumericalFunctionId = rarity.MaxAbilityLevelNumericalFunctionId,
            RequiredExpForLevelUpNumericalParameterMapId = rarity.RequiredExpForLevelUpNumericalParameterMapId,
            EnhancementCostByWeaponNumericalFunctionId = rarity.EnhancementCostByWeaponNumericalFunctionId,
            EnhancementCostByMaterialNumericalFunctionId = rarity.EnhancementCostByMaterialNumericalFunctionId,
            SkillEnhancementCostNumericalFunctionId = rarity.SkillEnhancementCostNumericalFunctionId,
            AbilityEnhancementCostNumericalFunctionId = rarity.AbilityEnhancementCostNumericalFunctionId,
            LimitBreakCostByWeaponNumericalFunctionId = rarity.LimitBreakCostByWeaponNumericalFunctionId,
            LimitBreakCostByMaterialNumericalFunctionId = rarity.LimitBreakCostByMaterialNumericalFunctionId,
            EvolutionCostNumericalFunctionId = rarity.EvolutionCostNumericalFunctionId
        };
    }

    /// <summary>
    /// The enhance parameters whose EXP table levels the weapon: those of its evolution chain's first weapon,
    /// so EXP carries over unchanged when the weapon evolves.
    /// </summary>
    public static EntityMWeaponSpecificEnhance? ResolveLevelingEnhance(DarkMasterMemoryDatabase masterDb, EntityMWeapon weaponMaster)
    {
        EntityMWeaponEvolutionGroup? member = masterDb.EntityMWeaponEvolutionGroup.FirstOrDefault(e => e.WeaponId == weaponMaster.WeaponId);
        if (member != null)
        {
            EntityMWeaponEvolutionGroup root = masterDb.EntityMWeaponEvolutionGroup
                .Where(e => e.WeaponEvolutionGroupId == member.WeaponEvolutionGroupId)
                .OrderBy(e => e.EvolutionOrder)
                .First();
            EntityMWeapon? rootMaster = masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponId == root.WeaponId);
            if (rootMaster != null)
            {
                return ResolveEnhance(masterDb, rootMaster);
            }
        }

        return ResolveEnhance(masterDb, weaponMaster);
    }

    /// <summary>
    /// Grants a weapon to the user: creates EntityIUserWeapon, EntityIUserWeaponNote (if new),
    /// ability/skill slots, and unlocks weapon stories for the ACQUISITION condition.
    /// </summary>
    public static void GrantWeapon(DarkUserMemoryDatabase userDb, int weaponId, DarkMasterMemoryDatabase masterDb, long nowMs)
    {
        string uuid = Guid.NewGuid().ToString();

        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = userDb.UserId,
            UserWeaponUuid = uuid,
            WeaponId = weaponId,
            Level = 1,
            AcquisitionDatetime = nowMs
        });

        // WeaponNote tracks per-weaponId metadata (max level, first acquisition); create only for the first copy
        if (!userDb.EntityIUserWeaponNote.Any(n => n.WeaponId == weaponId))
        {
            userDb.EntityIUserWeaponNote.Add(new EntityIUserWeaponNote
            {
                UserId = userDb.UserId,
                WeaponId = weaponId,
                MaxLevel = 1,
                FirstAcquisitionDatetime = nowMs
            });
        }

        // Look up master definition to populate ability/skill slots
        EntityMWeapon? masterWeapon = masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponId == weaponId);
        if (masterWeapon == null) return;

        // Create one ability slot per entry in the weapon's ability group
        foreach (EntityMWeaponAbilityGroup ag in masterDb.EntityMWeaponAbilityGroup.Where(g => g.WeaponAbilityGroupId == masterWeapon.WeaponAbilityGroupId))
            userDb.EntityIUserWeaponAbility.Add(new EntityIUserWeaponAbility { UserId = userDb.UserId, UserWeaponUuid = uuid, SlotNumber = ag.SlotNumber, Level = 1 });

        // Create one skill slot per entry in the weapon's skill group
        foreach (EntityMWeaponSkillGroup sg in masterDb.EntityMWeaponSkillGroup.Where(g => g.WeaponSkillGroupId == masterWeapon.WeaponSkillGroupId))
            userDb.EntityIUserWeaponSkill.Add(new EntityIUserWeaponSkill { UserId = userDb.UserId, UserWeaponUuid = uuid, SlotNumber = sg.SlotNumber, Level = 1 });

        // Unlock weapon stories for ACQUISITION condition
        if (masterWeapon.WeaponStoryReleaseConditionGroupId != 0)
        {
            foreach (EntityMWeaponStoryReleaseConditionGroup condRow in masterDb.EntityMWeaponStoryReleaseConditionGroup
                .Where(c => c.WeaponStoryReleaseConditionGroupId == masterWeapon.WeaponStoryReleaseConditionGroupId
                         && c.WeaponStoryReleaseConditionType == WeaponStoryReleaseConditionType.ACQUISITION
                         && c.ConditionValue == 0))
            {
                GrantWeaponStory(userDb, masterWeapon.WeaponId, condRow.StoryIndex);
            }
        }
    }

    /// <summary>Creates or updates a weapon story unlock record.</summary>
    public static void GrantWeaponStory(DarkUserMemoryDatabase userDb, int weaponId, int storyIndex)
    {
        EntityIUserWeaponStory? existing = userDb.EntityIUserWeaponStory.FirstOrDefault(s => s.WeaponId == weaponId);
        if (existing == null)
            userDb.EntityIUserWeaponStory.Add(new EntityIUserWeaponStory { UserId = userDb.UserId, WeaponId = weaponId, ReleasedMaxStoryIndex = storyIndex });
        else
            existing.ReleasedMaxStoryIndex = Math.Max(existing.ReleasedMaxStoryIndex, storyIndex);
    }
}
