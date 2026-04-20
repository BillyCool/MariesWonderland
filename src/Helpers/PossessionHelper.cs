using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Central handler for granting possessions to users. Consolidates the PossessionType switch
/// logic that was duplicated across QuestService, GachaService, RewardService, and TutorialService.
/// </summary>
public static class PossessionHelper
{
    /// <summary>
    /// Grants a possession to the user by type, delegating to type-specific handlers.
    /// </summary>
    public static void Apply(DarkUserMemoryDatabase userDb, PossessionType type, int id, int count, DarkMasterMemoryDatabase masterDb)
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        switch (type)
        {
            case PossessionType.FREE_GEM:
                userDb.EntityIUserGem[0].FreeGem += count;
                break;
            case PossessionType.PAID_GEM:
                userDb.EntityIUserGem[0].PaidGem += count;
                break;
            case PossessionType.MATERIAL:
                {
                    EntityIUserMaterial? mat = userDb.EntityIUserMaterial.FirstOrDefault(m => m.MaterialId == id);
                    if (mat == null)
                        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial { UserId = userDb.UserId, MaterialId = id, Count = count, FirstAcquisitionDatetime = nowMs });
                    else
                        mat.Count += count;
                    break;
                }
            case PossessionType.CONSUMABLE_ITEM:
                {
                    EntityIUserConsumableItem? item = userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == id);
                    if (item == null)
                        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem { UserId = userDb.UserId, ConsumableItemId = id, Count = count, FirstAcquisitionDatetime = nowMs });
                    else
                        item.Count += count;
                    break;
                }
            case PossessionType.IMPORTANT_ITEM:
                {
                    EntityIUserImportantItem? item = userDb.EntityIUserImportantItem.FirstOrDefault(c => c.ImportantItemId == id);
                    if (item == null)
                        userDb.EntityIUserImportantItem.Add(new EntityIUserImportantItem { UserId = userDb.UserId, ImportantItemId = id, Count = count, FirstAcquisitionDatetime = nowMs });
                    else
                        item.Count += count;
                    break;
                }
            case PossessionType.PREMIUM_ITEM:
                {
                    EntityIUserPremiumItem? item = userDb.EntityIUserPremiumItem.FirstOrDefault(p => p.PremiumItemId == id);
                    if (item == null)
                        userDb.EntityIUserPremiumItem.Add(new EntityIUserPremiumItem { UserId = userDb.UserId, PremiumItemId = id, AcquisitionDatetime = nowMs });
                    else
                        item.AcquisitionDatetime = nowMs;
                    break;
                }
            case PossessionType.WEAPON:
            case PossessionType.WEAPON_ENHANCED:
                WeaponHelper.GrantWeapon(userDb, id, masterDb);
                break;
            case PossessionType.COSTUME:
            case PossessionType.COSTUME_ENHANCED:
                GrantCostume(userDb, id, masterDb);
                break;
            case PossessionType.COMPANION:
                GrantCompanion(userDb, id);
                break;
            case PossessionType.PARTS:
                GrantParts(userDb, id, masterDb);
                break;
        }
    }

    /// <summary>
    /// Grants a costume to the user, unlocking the associated character if not already owned.
    /// </summary>
    public static void GrantCostume(DarkUserMemoryDatabase userDb, int costumeId, DarkMasterMemoryDatabase masterDb)
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string uuid = Guid.NewGuid().ToString();

        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = userDb.UserId,
            UserCostumeUuid = uuid,
            CostumeId = costumeId,
            Level = 1,
            HeadupDisplayViewId = 1,
            AcquisitionDatetime = nowMs
        });

        // Auto-unlock the character tied to this costume if not already owned
        EntityMCostume? masterCostume = masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == costumeId);
        if (masterCostume != null && !userDb.EntityIUserCharacter.Any(c => c.CharacterId == masterCostume.CharacterId))
            userDb.EntityIUserCharacter.Add(new EntityIUserCharacter { UserId = userDb.UserId, CharacterId = masterCostume.CharacterId, Level = 1 });

        userDb.EntityIUserCostumeActiveSkill.Add(new EntityIUserCostumeActiveSkill { UserId = userDb.UserId, UserCostumeUuid = uuid, Level = 1, AcquisitionDatetime = nowMs });
    }

    /// <summary>
    /// Grants a companion to the user. Skips if the companion is already owned.
    /// </summary>
    public static void GrantCompanion(DarkUserMemoryDatabase userDb, int companionId)
    {
        if (userDb.EntityIUserCompanion.Any(c => c.CompanionId == companionId)) return;

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        userDb.EntityIUserCompanion.Add(new EntityIUserCompanion
        {
            UserId = userDb.UserId,
            UserCompanionUuid = Guid.NewGuid().ToString(),
            CompanionId = companionId,
            Level = 1,
            HeadupDisplayViewId = 1,
            AcquisitionDatetime = nowMs
        });
    }

    /// <summary>
    /// Grants a single Parts item to the user. Skips if the user already owns a part with the same PartsId.
    /// </summary>
    public static void GrantParts(DarkUserMemoryDatabase userDb, int partsId, DarkMasterMemoryDatabase masterDb)
    {
        if (userDb.EntityIUserParts.Any(p => p.PartsId == partsId)) return;

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        EntityMParts? partsDef = masterDb.EntityMParts.FirstOrDefault(p => p.PartsId == partsId);
        // Derive main stat ID from the lottery group: tens digit = tier, ones digit = category
        int mainStatId = 0;
        if (partsDef != null)
        {
            int groupId = partsDef.PartsStatusMainLotteryGroupId;
            if (groupId > 0)
            {
                int tier = groupId / 10;
                int category = groupId % 10;
                mainStatId = (category - 1) * 4 + tier;
            }

            if (!userDb.EntityIUserPartsGroupNote.Any(n => n.PartsGroupId == partsDef.PartsGroupId))
            {
                userDb.EntityIUserPartsGroupNote.Add(new EntityIUserPartsGroupNote
                {
                    UserId = userDb.UserId,
                    PartsGroupId = partsDef.PartsGroupId,
                    FirstAcquisitionDatetime = nowMs
                });
            }
        }

        userDb.EntityIUserParts.Add(new EntityIUserParts
        {
            UserId = userDb.UserId,
            UserPartsUuid = Guid.NewGuid().ToString(),
            PartsId = partsId,
            Level = 1,
            PartsStatusMainId = mainStatId,
            AcquisitionDatetime = nowMs
        });
    }
}
