using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestTowerAccumulationReward))]
[PrimaryKey(nameof(EventQuestChapterId))]
public class EntityMEventQuestTowerAccumulationReward
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int EventQuestTowerAccumulationRewardGroupId { get; set; }
}
