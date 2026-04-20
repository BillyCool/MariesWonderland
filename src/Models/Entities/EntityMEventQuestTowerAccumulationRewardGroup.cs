using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestTowerAccumulationRewardGroup))]
public class EntityMEventQuestTowerAccumulationRewardGroup
{
    [Key(0)] public int EventQuestTowerAccumulationRewardGroupId { get; set; }

    [Key(1)] public int QuestMissionClearCount { get; set; }

    [Key(2)] public int EventQuestTowerRewardGroupId { get; set; }
}
