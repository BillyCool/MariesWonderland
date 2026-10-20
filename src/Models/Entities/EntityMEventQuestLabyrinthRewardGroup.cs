using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthRewardGroup))]
[PrimaryKey(nameof(EventQuestLabyrinthRewardGroupId), nameof(SortOrder))]
public class EntityMEventQuestLabyrinthRewardGroup
{
    [Key(0)] public int EventQuestLabyrinthRewardGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public PossessionType PossessionType { get; set; }

    [Key(3)] public int PossessionId { get; set; }

    [Key(4)] public int Count { get; set; }
}
