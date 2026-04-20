using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetBackground))]
public class EntityMAssetBackground
{
    [Key(0)] public int AssetBackgroundId { get; set; }

    [Key(1)] public string BackgroundAssetPath { get; set; }

    [Key(2)] public string GlobalEventMapAssetPath { get; set; }
}
