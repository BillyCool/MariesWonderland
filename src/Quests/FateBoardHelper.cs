using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Labyrinth;

namespace MariesWonderland.Quests;

/// <summary>
/// Fate Board (labyrinth event chapters) and Abyss Tower rules the client relies on. A board's quests are its chapter's
/// Normal sequence, split into stages by sequence sort order. Each season pays once, after it ends, for the furthest
/// cleared quest that has a season reward row (the client's "current reach reward"). The client shows a board only
/// while exactly one of its seasons is current.
/// </summary>
public static class FateBoardHelper
{
    /// <summary>The chapter's quests in play order with their sequence sort order (the given difficulty, or all).</summary>
    public static List<(int SortOrder, int QuestId)> ChapterQuests(DarkMasterMemoryDatabase masterDb, int eventQuestChapterId, DifficultyType? difficulty)
    {
        int groupId = masterDb.EntityMEventQuestChapter.FirstOrDefault(c => c.EventQuestChapterId == eventQuestChapterId)?.EventQuestSequenceGroupId ?? 0;
        List<int> sequenceIds = [.. masterDb.EntityMEventQuestSequenceGroup
            .Where(g => g.EventQuestSequenceGroupId == groupId && (difficulty == null || g.DifficultyType == difficulty))
            .OrderBy(g => g.DifficultyType)
            .Select(g => g.EventQuestSequenceId)];
        return [.. sequenceIds.SelectMany(id => masterDb.EntityMEventQuestSequence
            .Where(s => s.EventQuestSequenceId == id)
            .OrderBy(s => s.SortOrder)
            .Select(s => (s.SortOrder, s.QuestId)))];
    }

    /// <summary>A stage's quests: the board's Normal quests within the stage's sort order range.</summary>
    public static List<int> StageQuestIds(DarkMasterMemoryDatabase masterDb, EntityMEventQuestLabyrinthStage stage)
        => [.. ChapterQuests(masterDb, stage.EventQuestChapterId, DifficultyType.NORMAL)
            .Where(q => q.SortOrder >= stage.StartSequenceSortOrder && q.SortOrder <= stage.EndSequenceSortOrder)
            .Select(q => q.QuestId)];

    /// <summary>
    /// Cleared quest missions over the quests, as the client counts them for the board and tower milestones: every
    /// cleared mission except the "clear all missions" one.
    /// </summary>
    public static int ClearedQuestMissionCount(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, IEnumerable<int> questIds)
    {
        HashSet<int> quests = [.. questIds];
        HashSet<int> completeMissionIds = [.. masterDb.EntityMQuestMission
            .Where(m => m.QuestMissionConditionType == QuestMissionConditionType.COMPLETE)
            .Select(m => m.QuestMissionId)];
        return userDb.EntityIUserQuestMission.Count(m => m.IsClear && quests.Contains(m.QuestId) && !completeMissionIds.Contains(m.QuestMissionId));
    }

    public static bool IsQuestCleared(DarkUserMemoryDatabase userDb, int questId)
        => userDb.EntityIUserQuest.Any(q => q.QuestId == questId && q.ClearCount > 0);

    /// <summary>The board's current season: the one running now, or none when no season or more than one is.</summary>
    public static EntityMEventQuestLabyrinthSeason? CurrentSeason(DarkMasterMemoryDatabase masterDb, int eventQuestChapterId, long nowMs)
    {
        List<EntityMEventQuestLabyrinthSeason> running = [.. masterDb.EntityMEventQuestLabyrinthSeason
            .Where(s => s.EventQuestChapterId == eventQuestChapterId && s.StartDatetime <= nowMs && nowMs <= s.EndDatetime)];
        return running.Count == 1 ? running[0] : null;
    }

    /// <summary>Gift text 33: "Earned from Fate Board period achievement rewards."</summary>
    private const int SeasonRewardGiftTextId = 33;

    /// <summary>Records that the user is playing the board's current season.</summary>
    public static void JoinCurrentSeason(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, int eventQuestChapterId, long nowMs)
    {
        EntityMEventQuestLabyrinthSeason? season = CurrentSeason(masterDb, eventQuestChapterId, nowMs);
        if (season == null)
        {
            return;
        }

        EntityIUserEventQuestLabyrinthSeason row = userDb.EntityIUserEventQuestLabyrinthSeason.GetOrCreate(
            s => s.EventQuestChapterId == eventQuestChapterId,
            () => new EntityIUserEventQuestLabyrinthSeason { UserId = userDb.UserId, EventQuestChapterId = eventQuestChapterId });
        if (row.LastJoinSeasonNumber != season.SeasonNumber)
        {
            row.LastJoinSeasonNumber = season.SeasonNumber;
        }
    }

    /// <summary>
    /// Pays the last season the user joined on the board once it has ended and isn't paid yet: sends the reach
    /// reward for the furthest cleared quest to the gift box and returns it for the client's result screen. Null when there's nothing
    /// to pay (also when nothing is cleared, which still marks the season paid).
    /// </summary>
    public static LabyrinthSeasonResult? PayEndedSeason(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb,
        EntityIUserEventQuestLabyrinthSeason row, long nowMs)
    {
        if (row.LastJoinSeasonNumber == 0 || row.LastJoinSeasonNumber == row.LastSeasonRewardReceivedSeasonNumber)
        {
            return null;
        }

        EntityMEventQuestLabyrinthSeason? season = masterDb.EntityMEventQuestLabyrinthSeason
            .FirstOrDefault(s => s.EventQuestChapterId == row.EventQuestChapterId && s.SeasonNumber == row.LastJoinSeasonNumber);
        if (season == null || nowMs <= season.EndDatetime)
        {
            return null;
        }

        row.LastSeasonRewardReceivedSeasonNumber = row.LastJoinSeasonNumber;

        Dictionary<int, int> rewardGroupByHeadQuest = masterDb.EntityMEventQuestLabyrinthSeasonRewardGroup
            .Where(r => r.EventQuestLabyrinthSeasonRewardGroupId == season.SeasonRewardGroupId)
            .ToDictionary(r => r.HeadQuestId, r => r.EventQuestLabyrinthRewardGroupId);
        (int SortOrder, int QuestId) head = ChapterQuests(masterDb, row.EventQuestChapterId, DifficultyType.NORMAL)
            .LastOrDefault(q => rewardGroupByHeadQuest.ContainsKey(q.QuestId) && IsQuestCleared(userDb, q.QuestId));
        if (head.QuestId == 0)
        {
            return null;
        }

        int headStageOrder = masterDb.EntityMEventQuestLabyrinthStage
            .FirstOrDefault(s => s.EventQuestChapterId == row.EventQuestChapterId
                && head.SortOrder >= s.StartSequenceSortOrder && head.SortOrder <= s.EndSequenceSortOrder)?.StageOrder ?? 0;
        LabyrinthSeasonResult result = new()
        {
            EventQuestChapterId = row.EventQuestChapterId,
            HeadQuestId = head.QuestId,
            HeadStageOrder = headStageOrder
        };
        foreach (EntityMEventQuestLabyrinthRewardGroup reward in masterDb.EntityMEventQuestLabyrinthRewardGroup
            .Where(r => r.EventQuestLabyrinthRewardGroupId == rewardGroupByHeadQuest[head.QuestId])
            .OrderBy(r => r.SortOrder))
        {
            GiftBoxHelper.Add(userDb, reward.PossessionType, reward.PossessionId, reward.Count, nowMs, SeasonRewardGiftTextId);
            result.SeasonReward.Add(new LabyrinthReward { PossessionType = (int)reward.PossessionType, PossessionId = reward.PossessionId, Count = reward.Count });
        }

        return result;
    }
}
