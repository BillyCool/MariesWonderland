using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetEffect))]
[PrimaryKey(nameof(AssetEffectId))]
public class EntityMAssetEffect
{
    [Key(0)] public int AssetEffectId { get; set; }

    [Key(1)] public string AssetPath { get; set; }
}
