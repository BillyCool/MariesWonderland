using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetTimeline))]
[PrimaryKey(nameof(AssetTimelineId))]
public class EntityMAssetTimeline
{
    [Key(0)] public int AssetTimelineId { get; set; }

    [Key(1)] public string AssetPath { get; set; }
}
