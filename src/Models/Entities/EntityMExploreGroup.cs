using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExploreGroup))]
[PrimaryKey(nameof(ExploreGroupId), nameof(DifficultyType))]
public class EntityMExploreGroup
{
    [Key(0)] public int ExploreGroupId { get; set; }

    [Key(1)] public DifficultyType DifficultyType { get; set; }

    [Key(2)] public int ExploreId { get; set; }
}
