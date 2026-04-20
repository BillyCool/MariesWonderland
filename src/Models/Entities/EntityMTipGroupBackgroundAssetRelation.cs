using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipGroupBackgroundAssetRelation))]
public class EntityMTipGroupBackgroundAssetRelation
{
    [Key(0)] public int TipGroupId { get; set; }

    [Key(1)] public int TipBackgroundAssetId { get; set; }

    [Key(2)] public int TipDisplayConditionGroupId { get; set; }
}
