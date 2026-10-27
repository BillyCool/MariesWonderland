using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Cage gimmicks (black birds, Lost Items, hidden stories, Lost Archives, Stray Scarecrows, the fruit in Mama's Room).
/// The client only shows a gimmick once the user has an <c>IUserGimmick</c> row for it (and, on the map, an ornament
/// row with a <c>BaseDatetime</c>), so the server writes those rows when a sequence becomes current.
/// </summary>
public static class CageGimmickHelper
{
    private const long MillisecondsPerMinute = 60_000;

    /// <summary>
    /// Items piled up on an interval gimmick (Lost Items, the fruit in Mama's Room), as the client counts them: whole
    /// minutes since <paramref name="baseMs"/> divided by the interval (minutes), capped at the maximum.
    /// </summary>
    public static int AvailableIntervalCount(EntityMGimmickInterval interval, long baseMs, long nowMs)
    {
        if (interval.IntervalValue <= 0)
        {
            return 0;
        }

        long piled = (nowMs / MillisecondsPerMinute - baseMs / MillisecondsPerMinute) / interval.IntervalValue;
        return (int)Math.Clamp(piled, 0, interval.MaxValue);
    }

    /// <summary>The base time that gives an interval gimmick its initial pile (<c>InitialValue</c> items) right away.</summary>
    public static long InitialBaseDatetime(EntityMGimmickInterval? interval, long nowMs)
        => interval == null ? nowMs : nowMs - (long)interval.InitialValue * interval.IntervalValue * MillisecondsPerMinute;

    /// <summary>
    /// Adds the user rows for every gimmick of a current sequence that doesn't have them yet: <c>IUserGimmick</c> (not
    /// cleared, started now) and one <c>IUserGimmickOrnamentProgress</c> per ornament (no bits, base time now or the
    /// initial pile for interval gimmicks). A gimmick whose release condition fails is left out until it passes
    /// (the Lost Archives waiting on one of The Sun and the Moon III's). Existing rows are left alone.
    /// </summary>
    public static void SeedSequenceGimmicks(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, int scheduleId, int sequenceId, long nowMs)
    {
        EntityMGimmickSequence? sequence = masterDb.EntityMGimmickSequence.FirstOrDefault(s => s.GimmickSequenceId == sequenceId);
        if (sequence == null)
        {
            return;
        }

        List<int> gimmickIds = [.. masterDb.EntityMGimmickGroup
            .Where(g => g.GimmickGroupId == sequence.GimmickGroupId)
            .OrderBy(g => g.GroupIndex)
            .Select(g => g.GimmickId)];
        foreach (int gimmickId in gimmickIds)
        {
            EntityMGimmick? gimmick = masterDb.EntityMGimmick.FirstOrDefault(g => g.GimmickId == gimmickId);
            if (gimmick == null)
            {
                continue;
            }

            if (gimmick.ReleaseEvaluateConditionId != 0
                && EvaluateConditionHelper.Evaluate(gimmick.ReleaseEvaluateConditionId, userDb, masterDb) == false)
            {
                continue;
            }

            userDb.EntityIUserGimmick.GetOrCreate(
                g => g.GimmickSequenceScheduleId == scheduleId && g.GimmickSequenceId == sequenceId && g.GimmickId == gimmickId,
                () => new EntityIUserGimmick
                {
                    UserId = userDb.UserId,
                    GimmickSequenceScheduleId = scheduleId,
                    GimmickSequenceId = sequenceId,
                    GimmickId = gimmickId,
                    StartDatetime = nowMs
                });

            long baseDatetime = InitialBaseDatetime(masterDb.EntityMGimmickInterval.FirstOrDefault(i => i.GimmickId == gimmickId), nowMs);
            foreach (EntityMGimmickOrnament ornament in masterDb.EntityMGimmickOrnament.Where(o => o.GimmickOrnamentGroupId == gimmick.GimmickOrnamentGroupId))
            {
                userDb.EntityIUserGimmickOrnamentProgress.GetOrCreate(
                    o => o.GimmickSequenceScheduleId == scheduleId && o.GimmickSequenceId == sequenceId
                        && o.GimmickId == gimmickId && o.GimmickOrnamentIndex == ornament.GimmickOrnamentIndex,
                    () => new EntityIUserGimmickOrnamentProgress
                    {
                        UserId = userDb.UserId,
                        GimmickSequenceScheduleId = scheduleId,
                        GimmickSequenceId = sequenceId,
                        GimmickId = gimmickId,
                        GimmickOrnamentIndex = ornament.GimmickOrnamentIndex,
                        BaseDatetime = baseDatetime
                    });
            }
        }
    }

    /// <summary>Removes a sequence's gimmick rows that aren't cleared (its schedule ended before the user finished it).</summary>
    public static void RemoveUnfinishedSequenceGimmicks(DarkUserMemoryDatabase userDb, int scheduleId, int sequenceId)
    {
        HashSet<int> clearedGimmickIds = [.. userDb.EntityIUserGimmick
            .Where(g => g.GimmickSequenceScheduleId == scheduleId && g.GimmickSequenceId == sequenceId && g.IsGimmickCleared)
            .Select(g => g.GimmickId)];
        userDb.EntityIUserGimmick.RemoveAll(g => g.GimmickSequenceScheduleId == scheduleId && g.GimmickSequenceId == sequenceId && !g.IsGimmickCleared);
        userDb.EntityIUserGimmickOrnamentProgress.RemoveAll(o => o.GimmickSequenceScheduleId == scheduleId && o.GimmickSequenceId == sequenceId
            && !clearedGimmickIds.Contains(o.GimmickId));
    }

    /// <summary>
    /// What one fickle black bird, Lost Item or fruit from Mama's Room gives.
    /// REVIEW: placeholder. The live game drew these from a random loot pool that isn't in master data or any file we
    /// have; replace this once the pools are reviewed (docs/systems/cage.md, "Loot pools").
    /// </summary>
    public static (PossessionType PossessionType, int PossessionId, int Count) PlaceholderLoot(GameConfig gameConfig)
        => (PossessionType.CONSUMABLE_ITEM, gameConfig.ConsumableItemIdForGold, 1000);
}
