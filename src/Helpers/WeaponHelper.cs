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
    /// Grants a weapon to the user: creates EntityIUserWeapon, EntityIUserWeaponNote (if new),
    /// ability/skill slots, and unlocks weapon stories for the ACQUISITION condition.
    /// </summary>
    public static void GrantWeapon(DarkUserMemoryDatabase userDb, int weaponId, DarkMasterMemoryDatabase masterDb)
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
