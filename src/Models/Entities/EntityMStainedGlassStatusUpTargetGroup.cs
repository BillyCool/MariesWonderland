using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMStainedGlassStatusUpTargetGroup))]
[PrimaryKey(nameof(StainedGlassStatusUpTargetGroupId), nameof(GroupIndex))]
public class EntityMStainedGlassStatusUpTargetGroup
{
    [Key(0)] public int StainedGlassStatusUpTargetGroupId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public StainedGlassStatusUpTargetType StatusUpTargetType { get; set; }

    [Key(3)] public int TargetValue { get; set; }
}
