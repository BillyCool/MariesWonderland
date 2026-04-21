using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Evaluates unlock conditions for various game systems against the current user's save data.
/// </summary>
public static class UnlockConditionHelper
{
    /// <summary>
    /// Returns true if the given explore unlock condition is satisfied for the specified user.
    /// Throws <see cref="RpcException"/> with <see cref="StatusCode.Internal"/> for unhandled condition types.
    /// </summary>
    public static bool IsExploreUnlocked(int exploreId, EntityMExploreUnlockCondition condition,
        DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb)
    {
        return condition.ExploreUnlockConditionType switch
        {
            ExploreUnlockConditionType.MAIN_QUEST_CLEAR =>
                IsQuestCleared(userDb, condition.ConditionValue),

            ExploreUnlockConditionType.EXPLORE_SCORE_OVER_IN_SAME_GROUP_AND_ONE_LOW_DIFFICULTY =>
                HasScoreInLowerDifficulty(exploreId, condition.ConditionValue, userDb, masterDb),

            _ => throw new RpcException(new Status(StatusCode.Internal,
                $"Unhandled ExploreUnlockConditionType: {condition.ExploreUnlockConditionType}"))
        };
    }

    private static bool IsQuestCleared(DarkUserMemoryDatabase userDb, int questId)
        => userDb.EntityIUserQuest.Any(q => q.QuestId == questId && q.QuestStateType == (int)QuestStateType.CLEARED);

    private static bool HasScoreInLowerDifficulty(int exploreId, int requiredScore,
        DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb)
    {
        EntityMExploreGroup? groupEntry = masterDb.EntityMExploreGroup
            .FirstOrDefault(g => g.ExploreId == exploreId);

        if (groupEntry is null) return false;

        EntityMExploreGroup? lowerDiffEntry = masterDb.EntityMExploreGroup
            .FirstOrDefault(g => g.ExploreGroupId == groupEntry.ExploreGroupId
                              && g.DifficultyType == groupEntry.DifficultyType - 1);

        if (lowerDiffEntry is null) return false;

        EntityIUserExploreScore? score = userDb.EntityIUserExploreScore
            .FirstOrDefault(s => s.ExploreId == lowerDiffEntry.ExploreId);

        return score is not null && score.MaxScore >= requiredScore;
    }
}
