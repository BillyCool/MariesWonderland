using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// A memoir's sub-status slots (<c>IUserPartsStatusSub</c>). A memoir starts with the slots its grade gives, each a
/// first roll at level 1. Every <see cref="MemoirTable.UpgradeEveryLevels"/> levels (3, 6, 9, 12, 15) it fills an empty
/// slot, or adds another roll to one of the filled slots. A slot's <c>Level</c> is its number of rolls and its value
/// the sum of them.
/// </summary>
public static class MemoirSubStatusHelper
{
    /// <summary>Rolls the starting slots of a new memoir at level 1 and adds them to the user's rows.</summary>
    public static void AddStartingSubStatuses(DarkUserMemoryDatabase userDb, EntityIUserParts parts, EntityMParts partsDef, MemoirTable table, Random random)
    {
        int slots = table.RollStartingSlots(partsDef.PartsInitialLotteryId, random);
        for (int i = 0; i < slots; i++)
        {
            if (!TryFillSlot(userDb, parts, partsDef.RarityType, table, random))
            {
                break;
            }
        }
    }

    /// <summary>The memoir went from <paramref name="fromLevel"/> up to <paramref name="toLevel"/>: fills or upgrades a slot for every upgrade level it passed.</summary>
    public static void AdvanceLevel(DarkUserMemoryDatabase userDb, EntityIUserParts parts, RarityType rarity, int fromLevel, int toLevel, MemoirTable table, Random random)
    {
        for (int level = fromLevel + 1; level <= toLevel; level++)
        {
            if (level % table.UpgradeEveryLevels != 0)
            {
                continue;
            }

            List<EntityIUserPartsStatusSub> filled = SlotsOf(userDb, parts);
            if (filled.Count < table.MaxSlots)
            {
                TryFillSlot(userDb, parts, rarity, table, random);
            }
            else if (filled.Count > 0)
            {
                UpgradeSlot(filled[random.Next(filled.Count)], rarity, table, random);
            }
        }
    }

    private static List<EntityIUserPartsStatusSub> SlotsOf(DarkUserMemoryDatabase userDb, EntityIUserParts parts)
        => [.. userDb.EntityIUserPartsStatusSub.Where(s => s.UserPartsUuid == parts.UserPartsUuid)];

    private static bool TryFillSlot(DarkUserMemoryDatabase userDb, EntityIUserParts parts, RarityType rarity, MemoirTable table, Random random)
    {
        List<EntityIUserPartsStatusSub> filled = SlotsOf(userDb, parts);
        MemoirSubStatusRoll? roll = table.PickNewRoll(rarity, [.. filled.Select(s => s.StatusKindType)], random);
        if (roll == null)
        {
            return false;
        }

        int statusIndex = 1;
        while (filled.Any(s => s.StatusIndex == statusIndex))
        {
            statusIndex++;
        }

        userDb.EntityIUserPartsStatusSub.Add(new EntityIUserPartsStatusSub
        {
            UserId = parts.UserId,
            UserPartsUuid = parts.UserPartsUuid,
            StatusIndex = statusIndex,
            PartsStatusSubLotteryId = roll.PartsStatusSubLotteryId,
            Level = 1,
            StatusKindType = roll.StatusKindType,
            StatusCalculationType = roll.StatusCalculationType,
            StatusChangeValue = roll.RollValue(random)
        });
        return true;
    }

    private static void UpgradeSlot(EntityIUserPartsStatusSub slot, RarityType rarity, MemoirTable table, Random random)
    {
        MemoirSubStatusRoll? roll = table.FindRoll(rarity, slot.PartsStatusSubLotteryId);
        if (roll == null)
        {
            return;
        }

        slot.Level++;
        slot.StatusChangeValue += roll.RollValue(random);
    }
}
