using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetTurnbattlePrefab))]
[PrimaryKey(nameof(AssetTurnbattlePrefabId))]
public class EntityMAssetTurnbattlePrefab
{
    [Key(0)] public int AssetTurnbattlePrefabId { get; set; }

    [Key(1)] public string AssetPath { get; set; }
}
