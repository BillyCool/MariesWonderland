using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetTurnbattlePrefab))]
public class EntityMAssetTurnbattlePrefab
{
    [Key(0)] public int AssetTurnbattlePrefabId { get; set; }

    [Key(1)] public string AssetPath { get; set; }
}
