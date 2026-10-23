using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Costumes;

/// <summary>
/// Costume karma (lottery effects). Costumes that support it have three slots (Karma I–III). Each slot is
/// unlocked in order with the costume's karma stones and gold, then rolled with Karma Flux for one effect from
/// the slot's odds group: slot I rolls a pair of stat boosts, slots II and III an ability. A flux on an empty slot
/// applies at once; on a filled slot the result waits as the costume's pending result until it is kept or
/// declined. The effects are also written out per costume: one ability row per ability slot and one stat row per
/// calculation type (flat or percent).
/// </summary>
public static class CostumeKarmaHelper
{
    /// <summary>Unlocks a slot when the previous one is unlocked and the stones and gold are there. Returns whether it did.</summary>
    public static bool UnlockSlot(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, GameConfig config, string userCostumeUuid, int slotNumber, long nowMs)
    {
        EntityMCostumeLotteryEffect? slot = FindReleasedSlot(userDb, masterDb, userCostumeUuid, slotNumber, nowMs);
        if (slot == null || FindUserSlot(userDb, userCostumeUuid, slotNumber) != null)
        {
            return false;
        }

        if (slotNumber > 1 && FindUserSlot(userDb, userCostumeUuid, slotNumber - 1) == null)
        {
            return false;
        }

        if (!TryPay(userDb, masterDb, config, slot.CostumeLotteryEffectUnlockMaterialGroupId, config.CostumeLotteryEffectUnlockSlotConsumeGold))
        {
            return false;
        }

        userDb.EntityIUserCostumeLotteryEffect.Add(new EntityIUserCostumeLotteryEffect
        {
            UserId = userDb.UserId,
            UserCostumeUuid = userCostumeUuid,
            SlotNumber = slotNumber,
            OddsNumber = 0
        });
        return true;
    }

    /// <summary>
    /// Karma Flux on an unlocked slot: pays the draw materials and gold and rolls the slot's odds group. An empty
    /// slot takes the result; a filled one keeps its effect and the result becomes the costume's pending result,
    /// replacing any earlier one. Returns whether a roll happened.
    /// </summary>
    public static bool Draw(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, GameConfig config, string userCostumeUuid, int slotNumber, Random random, long nowMs)
    {
        EntityMCostumeLotteryEffect? slot = FindReleasedSlot(userDb, masterDb, userCostumeUuid, slotNumber, nowMs);
        EntityIUserCostumeLotteryEffect? userSlot = FindUserSlot(userDb, userCostumeUuid, slotNumber);
        if (slot == null || userSlot == null)
        {
            return false;
        }

        List<EntityMCostumeLotteryEffectOddsGroup> odds = [.. masterDb.EntityMCostumeLotteryEffectOddsGroup
            .Where(o => o.CostumeLotteryEffectOddsGroupId == slot.CostumeLotteryEffectOddsGroupId && o.Weight > 0)];
        if (odds.Count == 0)
        {
            return false;
        }

        if (!TryPay(userDb, masterDb, config, slot.CostumeLotteryEffectDrawMaterialGroupId, config.CostumeLotteryEffectDrawSlotConsumeGold))
        {
            return false;
        }

        int oddsNumber = PickWeighted(odds, random).OddsNumber;
        if (userSlot.OddsNumber == 0)
        {
            userSlot.OddsNumber = oddsNumber;
            RefreshEffects(userDb, masterDb, userCostumeUuid);
            return true;
        }

        EntityIUserCostumeLotteryEffectPending? pending = FindPending(userDb, userCostumeUuid);
        if (pending == null)
        {
            userDb.EntityIUserCostumeLotteryEffectPending.Add(new EntityIUserCostumeLotteryEffectPending
            {
                UserId = userDb.UserId,
                UserCostumeUuid = userCostumeUuid,
                SlotNumber = slotNumber,
                OddsNumber = oddsNumber
            });
        }
        else
        {
            pending.SlotNumber = slotNumber;
            pending.OddsNumber = oddsNumber;
        }
        return true;
    }

    /// <summary>Keeps (<paramref name="isAccept"/>) or declines the costume's pending result, then clears it. Returns whether there was one.</summary>
    public static bool Confirm(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, string userCostumeUuid, bool isAccept)
    {
        EntityIUserCostumeLotteryEffectPending? pending = FindPending(userDb, userCostumeUuid);
        if (pending == null)
        {
            return false;
        }

        userDb.EntityIUserCostumeLotteryEffectPending.Remove(pending);
        EntityIUserCostumeLotteryEffect? userSlot = FindUserSlot(userDb, userCostumeUuid, pending.SlotNumber);
        if (isAccept && userSlot != null)
        {
            userSlot.OddsNumber = pending.OddsNumber;
            RefreshEffects(userDb, masterDb, userCostumeUuid);
        }
        return true;
    }

    /// <summary>
    /// Rewrites the costume's ability and stat rows from its slots' current results, updating rows in place and
    /// dropping the ones no slot gives any more.
    /// </summary>
    public static void RefreshEffects(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, string userCostumeUuid)
    {
        EntityIUserCostume? costume = FindCostume(userDb, userCostumeUuid);
        if (costume == null)
        {
            return;
        }

        Dictionary<int, EntityMCostumeLotteryEffectTargetAbility> abilities = [];
        Dictionary<StatusCalculationType, int[]> stats = [];
        foreach (EntityIUserCostumeLotteryEffect userSlot in userDb.EntityIUserCostumeLotteryEffect)
        {
            if (userSlot.UserCostumeUuid != userCostumeUuid || userSlot.OddsNumber == 0)
            {
                continue;
            }

            EntityMCostumeLotteryEffectOddsGroup? result = FindResult(masterDb, costume.CostumeId, userSlot.SlotNumber, userSlot.OddsNumber);
            if (result == null)
            {
                continue;
            }

            if (result.CostumeLotteryEffectType == CostumeLotteryEffectType.ABILITY)
            {
                EntityMCostumeLotteryEffectTargetAbility? ability = masterDb.EntityMCostumeLotteryEffectTargetAbility.Find(result.CostumeLotteryEffectTargetId);
                if (ability != null)
                {
                    abilities[userSlot.SlotNumber] = ability;
                }
                continue;
            }

            foreach (EntityMCostumeLotteryEffectTargetStatusUp statusUp in masterDb.EntityMCostumeLotteryEffectTargetStatusUp)
            {
                if (statusUp.CostumeLotteryEffectTargetStatusUpId != result.CostumeLotteryEffectTargetId)
                {
                    continue;
                }

                int index = StatIndex(statusUp.StatusKindType);
                if (index < 0)
                {
                    continue;
                }

                if (!stats.TryGetValue(statusUp.StatusCalculationType, out int[]? values))
                {
                    values = new int[6];
                    stats[statusUp.StatusCalculationType] = values;
                }
                values[index] += statusUp.EffectValue;
            }
        }

        userDb.EntityIUserCostumeLotteryEffectAbility.RemoveAll(r => r.UserCostumeUuid == userCostumeUuid && !abilities.ContainsKey(r.SlotNumber));
        foreach ((int slotNumber, EntityMCostumeLotteryEffectTargetAbility ability) in abilities)
        {
            EntityIUserCostumeLotteryEffectAbility? row = userDb.EntityIUserCostumeLotteryEffectAbility
                .FirstOrDefault(r => r.UserCostumeUuid == userCostumeUuid && r.SlotNumber == slotNumber);
            if (row == null)
            {
                row = new EntityIUserCostumeLotteryEffectAbility { UserId = userDb.UserId, UserCostumeUuid = userCostumeUuid, SlotNumber = slotNumber };
                userDb.EntityIUserCostumeLotteryEffectAbility.Add(row);
            }
            row.AbilityId = ability.AbilityId;
            row.AbilityLevel = ability.AbilityLevel;
        }

        userDb.EntityIUserCostumeLotteryEffectStatusUp.RemoveAll(r => r.UserCostumeUuid == userCostumeUuid && !stats.ContainsKey(r.StatusCalculationType));
        foreach ((StatusCalculationType calculationType, int[] values) in stats)
        {
            EntityIUserCostumeLotteryEffectStatusUp? row = userDb.EntityIUserCostumeLotteryEffectStatusUp
                .FirstOrDefault(r => r.UserCostumeUuid == userCostumeUuid && r.StatusCalculationType == calculationType);
            if (row == null)
            {
                row = new EntityIUserCostumeLotteryEffectStatusUp { UserId = userDb.UserId, UserCostumeUuid = userCostumeUuid, StatusCalculationType = calculationType };
                userDb.EntityIUserCostumeLotteryEffectStatusUp.Add(row);
            }
            row.Hp = values[0];
            row.Attack = values[1];
            row.Vitality = values[2];
            row.Agility = values[3];
            row.CriticalRatio = values[4];
            row.CriticalAttack = values[5];
        }
    }

    /// <summary>The odds row a slot's result points at, or null when the costume, slot or result isn't in master data.</summary>
    public static EntityMCostumeLotteryEffectOddsGroup? FindResult(DarkMasterMemoryDatabase masterDb, int costumeId, int slotNumber, int oddsNumber)
    {
        EntityMCostumeLotteryEffect? slot = masterDb.EntityMCostumeLotteryEffect.Find(costumeId, slotNumber);
        return slot == null ? null : masterDb.EntityMCostumeLotteryEffectOddsGroup.Find(slot.CostumeLotteryEffectOddsGroupId, oddsNumber);
    }

    /// <summary>Position of a stat in the stat row's columns (HP, attack, defense, agility, crit rate, crit damage), or -1.</summary>
    private static int StatIndex(StatusKindType kind) => kind switch
    {
        StatusKindType.HP => 0,
        StatusKindType.ATTACK => 1,
        StatusKindType.VITALITY => 2,
        StatusKindType.AGILITY => 3,
        StatusKindType.CRITICAL_RATIO => 4,
        StatusKindType.CRITICAL_ATTACK => 5,
        _ => -1
    };

    private static EntityMCostumeLotteryEffectOddsGroup PickWeighted(List<EntityMCostumeLotteryEffectOddsGroup> odds, Random random)
    {
        int roll = random.Next(odds.Sum(o => o.Weight));
        foreach (EntityMCostumeLotteryEffectOddsGroup o in odds)
        {
            roll -= o.Weight;
            if (roll < 0)
            {
                return o;
            }
        }
        return odds[^1];
    }

    /// <summary>Takes every material of the group and the gold, or nothing when any of it is short.</summary>
    private static bool TryPay(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, GameConfig config, int materialGroupId, int gold)
    {
        List<(EntityIUserMaterial Owned, int Count)> costs = [];
        foreach (EntityMCostumeLotteryEffectMaterialGroup cost in masterDb.EntityMCostumeLotteryEffectMaterialGroup)
        {
            if (cost.CostumeLotteryEffectMaterialGroupId != materialGroupId)
            {
                continue;
            }

            EntityIUserMaterial? owned = userDb.EntityIUserMaterial.FirstOrDefault(m => m.MaterialId == cost.MaterialId);
            if (owned == null || owned.Count < cost.Count)
            {
                return false;
            }
            costs.Add((owned, cost.Count));
        }

        if (costs.Count == 0)
        {
            return false;
        }

        EntityIUserConsumableItem? goldRow = userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == config.ConsumableItemIdForGold);
        if (gold > 0 && (goldRow == null || goldRow.Count < gold))
        {
            return false;
        }

        foreach ((EntityIUserMaterial owned, int count) in costs)
        {
            owned.Count -= count;
        }

        if (goldRow != null)
        {
            goldRow.Count -= gold;
        }
        return true;
    }

    private static EntityMCostumeLotteryEffect? FindReleasedSlot(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, string userCostumeUuid, int slotNumber, long nowMs)
    {
        EntityIUserCostume? costume = FindCostume(userDb, userCostumeUuid);
        EntityMCostumeLotteryEffect? slot = costume == null ? null : masterDb.EntityMCostumeLotteryEffect.Find(costume.CostumeId, slotNumber);
        if (slot == null)
        {
            return null;
        }

        EntityMCostumeLotteryEffectReleaseSchedule? release = masterDb.EntityMCostumeLotteryEffectReleaseSchedule.Find(slot.CostumeLotteryEffectReleaseScheduleId);
        return release == null || release.ReleaseDatetime <= nowMs ? slot : null;
    }

    private static EntityIUserCostume? FindCostume(DarkUserMemoryDatabase userDb, string userCostumeUuid)
        => userDb.EntityIUserCostume.FirstOrDefault(c => c.UserCostumeUuid == userCostumeUuid);

    private static EntityIUserCostumeLotteryEffect? FindUserSlot(DarkUserMemoryDatabase userDb, string userCostumeUuid, int slotNumber)
        => userDb.EntityIUserCostumeLotteryEffect.FirstOrDefault(r => r.UserCostumeUuid == userCostumeUuid && r.SlotNumber == slotNumber);

    private static EntityIUserCostumeLotteryEffectPending? FindPending(DarkUserMemoryDatabase userDb, string userCostumeUuid)
        => userDb.EntityIUserCostumeLotteryEffectPending.FirstOrDefault(r => r.UserCostumeUuid == userCostumeUuid);
}
