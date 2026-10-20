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
            case PossessionType.PARTS_ENHANCED:
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
    /// Grants a single Parts item to the user.
    /// </summary>
    public static void GrantParts(DarkUserMemoryDatabase userDb, int partsId, DarkMasterMemoryDatabase masterDb)
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        CreateGrantedParts(userDb, ResolveGrantedPartsVariant(partsId, masterDb), partsId, nowMs);
    }

    /// <summary>
    /// Grants a quest-drop memoir variant or auto-sells it into gold when the user's rules match the rolled rarity and rank.
    /// </summary>
    public static (int GrantedPartsId, bool IsAutoSold) GrantOrSellPartsDrop(
        DarkUserMemoryDatabase userDb,
        int requestedPartsId,
        DarkMasterMemoryDatabase masterDb,
        int goldConsumableItemId,
        ISet<int> autoSaleRaritySet,
        ISet<int> autoSaleRankSet)
    {
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        EntityMParts? grantedParts = ResolveGrantedPartsVariant(requestedPartsId, masterDb);
        if (grantedParts == null)
        {
            CreateGrantedParts(userDb, null, requestedPartsId, nowMs);
            return (requestedPartsId, false);
        }

        bool shouldAutoSell = autoSaleRaritySet.Contains((int)grantedParts.RarityType)
            && autoSaleRankSet.Contains(grantedParts.PartsInitialLotteryId);

        int sellPrice = shouldAutoSell ? ResolvePartsSellPrice(masterDb, grantedParts.RarityType, 1) : 0;
        if (sellPrice > 0)
        {
            Apply(userDb, PossessionType.CONSUMABLE_ITEM, goldConsumableItemId, sellPrice, masterDb);
            return (grantedParts.PartsId, true);
        }

        CreateGrantedParts(userDb, grantedParts, grantedParts.PartsId, nowMs);
        return (grantedParts.PartsId, false);
    }

    private static EntityMParts? ResolveGrantedPartsVariant(int requestedPartsId, DarkMasterMemoryDatabase masterDb)
    {
        EntityMParts? requested = masterDb.EntityMParts.FirstOrDefault(p => p.PartsId == requestedPartsId);
        if (requested == null)
        {
            return null;
        }

        List<EntityMParts> variants = [.. masterDb.EntityMParts
            .Where(p => p.PartsGroupId == requested.PartsGroupId && p.RarityType == requested.RarityType)
            .OrderBy(p => p.PartsId)];

        if (variants.Count == 5)
        {
            return variants[Random.Shared.Next(variants.Count)];
        }

        return requested;
    }

    private static void CreateGrantedParts(DarkUserMemoryDatabase userDb, EntityMParts? partsDef, int partsId, long nowMs)
    {
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

            // TODO: Roll initial memoir sub-statuses for higher-rank drops. Needs the sub-status pool
            // from master data, which isn't modelled yet, so only the base part row is created for now.
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

    private static int ResolvePartsSellPrice(DarkMasterMemoryDatabase masterDb, RarityType rarityType, int level)
    {
        EntityMPartsRarity? rarityRow = masterDb.EntityMPartsRarity.FirstOrDefault(r => r.RarityType == rarityType);
        if (rarityRow == null || rarityRow.SellPriceNumericalFunctionId == 0)
        {
            return 0;
        }

        return EvaluateNumericalFunction(masterDb, rarityRow.SellPriceNumericalFunctionId, level);
    }

    private static int EvaluateNumericalFunction(DarkMasterMemoryDatabase masterDb, int functionId, int value)
    {
        EntityMNumericalFunction? func = masterDb.EntityMNumericalFunction.FirstOrDefault(f => f.NumericalFunctionId == functionId);
        if (func == null)
        {
            return 0;
        }

        int[] p = [.. masterDb.EntityMNumericalFunctionParameterGroup
            .Where(pg => pg.NumericalFunctionParameterGroupId == func.NumericalFunctionParameterGroupId)
            .OrderBy(pg => pg.ParameterIndex)
            .Select(pg => pg.ParameterValue)];

        return func.NumericalFunctionType switch
        {
            NumericalFunctionType.LINEAR when p.Length >= 2 => p[1] + p[0] * value,
            NumericalFunctionType.MONOMIAL when p.Length >= 2 => EvaluateMonomial(p, value),
            NumericalFunctionType.LINEAR_PERMIL when p.Length >= 2 => p[0] * value / 1000 + p[1],
            NumericalFunctionType.POLYNOMIAL_THIRD when p.Length >= 4 => p[3] + (p[2] + (p[1] + p[0] * value) * value) * value,
            NumericalFunctionType.POLYNOMIAL_THIRD_PERMIL when p.Length >= 4 =>
                p[0] * value * value * value / 1000 +
                p[1] * value * value / 1000 +
                p[2] * value / 1000 +
                p[3],
            _ => 0
        };
    }

    private static int EvaluateMonomial(int[] p, int value)
    {
        int v = value - 1;
        int result = v;
        int counter = p[1];
        while (counter > 1)
        {
            result *= v;
            counter--;
        }

        return p[0] * result;
    }
}
