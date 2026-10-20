using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestPickupRewardGroup))]
[PrimaryKey(nameof(QuestPickupRewardGroupId), nameof(SortOrder))]
public class EntityMQuestPickupRewardGroup
{
    [Key(0)] public int QuestPickupRewardGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BattleDropRewardId { get; set; }
}
