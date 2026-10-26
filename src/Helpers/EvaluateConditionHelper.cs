using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using System.Numerics;

namespace MariesWonderland.Helpers;

/// <summary>
/// Evaluates <c>EntityMEvaluateCondition</c> rows (gimmick schedules, report and cage-memory gimmicks) against the
/// user's save. Supported: quest clear, mission clear, quest-mission clear, another gimmick's bit count, and AND / OR
/// over other conditions. The value groups hold, in index order: the quest id; the mission id; the quest id then quest
/// mission id; the schedule, sequence, gimmick and ornament index then the minimum number of bits set; or the
/// condition ids to combine. Checked against the real save: every hidden-story report it cleared passes, every report
/// schedule it holds has its release quest cleared, and every one it lacks doesn't.
/// </summary>
public static class EvaluateConditionHelper
{
    private const int MaxDepth = 8;

    /// <summary>
    /// True or false for a supported condition; null when the condition (or one it combines) is of a kind not
    /// supported yet (scene choices…), or the id doesn't exist.
    /// </summary>
    public static bool? Evaluate(int evaluateConditionId, DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb)
        => Evaluate(evaluateConditionId, userDb, masterDb, 0);

    private static bool? Evaluate(int evaluateConditionId, DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, int depth)
    {
        EntityMEvaluateCondition? condition = masterDb.EntityMEvaluateCondition.Find(evaluateConditionId);
        if (condition == null || depth > MaxDepth)
        {
            return null;
        }

        List<long> values = [.. masterDb.EntityMEvaluateConditionValueGroup
            .Where(v => v.EvaluateConditionValueGroupId == condition.EvaluateConditionValueGroupId)
            .OrderBy(v => v.GroupIndex)
            .Select(v => v.Value)];
        if (values.Count == 0)
        {
            return null;
        }

        return (condition.EvaluateConditionFunctionType, condition.EvaluateConditionEvaluateType) switch
        {
            (EvaluateConditionFunctionType.RECURSION, EvaluateConditionEvaluateType.AND) => Combine(values, all: true, userDb, masterDb, depth),
            (EvaluateConditionFunctionType.RECURSION, EvaluateConditionEvaluateType.OR) => Combine(values, all: false, userDb, masterDb, depth),
            (EvaluateConditionFunctionType.QUEST_CLEAR, EvaluateConditionEvaluateType.ID_CONTAIN)
                => userDb.EntityIUserQuest.Any(q => q.QuestId == values[0] && q.ClearCount > 0),
            (EvaluateConditionFunctionType.MISSION_CLEAR, EvaluateConditionEvaluateType.ID_CONTAIN)
                => userDb.EntityIUserMission.Any(m => m.MissionId == values[0]
                    && m.MissionProgressStatusType is MissionProgressStatusType.CLEAR or MissionProgressStatusType.REWARD_RECEIVED),
            (EvaluateConditionFunctionType.QUEST_MISSION_CLEAR, EvaluateConditionEvaluateType.ID_CONTAIN) when values.Count >= 2
                => userDb.EntityIUserQuestMission.Any(m => m.QuestId == values[0] && m.QuestMissionId == values[1] && m.IsClear),
            (EvaluateConditionFunctionType.OTHER_GIMMICK_BIT_COUNT, EvaluateConditionEvaluateType.GE) when values.Count >= 5
                => userDb.EntityIUserGimmickOrnamentProgress.Any(o => o.GimmickSequenceScheduleId == values[0] && o.GimmickSequenceId == values[1]
                    && o.GimmickId == values[2] && o.GimmickOrnamentIndex == values[3]
                    && BitOperations.PopCount((uint)o.ProgressValueBit) >= values[4]),
            _ => null
        };
    }

    /// <summary>AND / OR over the conditions listed; unknown if any of them is.</summary>
    private static bool? Combine(List<long> conditionIds, bool all, DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, int depth)
    {
        List<bool?> results = [.. conditionIds.Select(id => Evaluate((int)id, userDb, masterDb, depth + 1))];
        if (results.Any(r => r == null))
        {
            return null;
        }

        return all ? results.All(r => r == true) : results.Any(r => r == true);
    }
}
