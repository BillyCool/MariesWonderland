using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthStageAccumulationRewardGroup))]
[PrimaryKey(nameof(EventQuestLabyrinthStageAccumulationRewardGroupId), nameof(QuestMissionClearCount))]
public class EntityMEventQuestLabyrinthStageAccumulationRewardGroup
{
    [Key(0)] public int EventQuestLabyrinthStageAccumulationRewardGroupId { get; set; }

    [Key(1)] public int QuestMissionClearCount { get; set; }

    [Key(2)] public int EventQuestLabyrinthRewardGroupId { get; set; }
}
