using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(QuestId))]
public class EntityIUserQuest : IUserEntity
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
