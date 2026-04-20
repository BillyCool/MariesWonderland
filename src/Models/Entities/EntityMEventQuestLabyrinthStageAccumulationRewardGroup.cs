using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthStageAccumulationRewardGroup))]
public class EntityMEventQuestLabyrinthStageAccumulationRewardGroup
{
    [Key(0)] public int EventQuestLabyrinthStageAccumulationRewardGroupId { get; set; }

    [Key(1)] public int QuestMissionClearCount { get; set; }

    [Key(2)] public int EventQuestLabyrinthRewardGroupId { get; set; }
}
