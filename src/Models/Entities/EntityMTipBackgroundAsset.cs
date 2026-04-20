using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipBackgroundAsset))]
public class EntityMTipBackgroundAsset
{
    [Key(0)] public int TipBackgroundAssetId { get; set; }

    [Key(1)] public string BackgroundAssetName { get; set; }
}
