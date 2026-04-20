using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetCalculator))]
public class EntityMAssetCalculator
{
    [Key(0)] public int AssetCalculatorId { get; set; }

    [Key(1)] public int UseCalculatorType { get; set; }

    [Key(2)] public string AssetPath { get; set; }
}
