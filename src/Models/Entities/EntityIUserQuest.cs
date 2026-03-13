using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserQuest
{
    public long UserId { get; set; }

    public int QuestId { get; set; }

    public int QuestStateType { get; set; }

    public bool IsBattleOnly { get; set; }

    public long LatestStartDatetime { get; set; }

    public int ClearCount { get; set; }

    public int DailyClearCount { get; set; }

    public long LastClearDatetime { get; set; }

    public int ShortestClearFrames { get; set; }

    public long LatestVersion { get; set; }
}
