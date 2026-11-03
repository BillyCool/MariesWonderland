using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Quests;

/// <summary>The reward lists and big-win outcome of finishing a quest, shared by every quest type's response.</summary>
public sealed class QuestFinishResult
{
    public List<QuestReward> DropRewards { get; init; } = [];
    public List<QuestReward> FirstClearRewards { get; init; } = [];
    public List<QuestReward> MissionClearRewards { get; init; } = [];
    public List<QuestReward> MissionClearCompleteRewards { get; init; } = [];
    public bool IsBigWin { get; init; }

    /// <summary>The COMPLETE-condition mission cleared by this run; 0 when <see cref="IsBigWin"/> is false.</summary>
    public int BigWinMissionId { get; init; }

    /// <summary>The missions the force bonus cleared without their condition, then the COMPLETE mission when this run cleared it.</summary>
    public List<int> BigWinClearedMissionIds { get; init; } = [];
}
