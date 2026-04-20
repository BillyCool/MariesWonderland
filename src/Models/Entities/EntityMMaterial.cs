using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMaterial))]
public class EntityMMaterial
{
    [Key(0)] public int MaterialId { get; set; }

    [Key(1)] public MaterialType MaterialType { get; set; }

    [Key(2)] public RarityType RarityType { get; set; }

    [Key(3)] public WeaponType WeaponType { get; set; }

    [Key(4)] public AttributeType AttributeType { get; set; }

    [Key(5)] public int EffectValue { get; set; }

    [Key(6)] public int SellPrice { get; set; }

    [Key(7)] public string AssetName { get; set; }

    [Key(8)] public int AssetCategoryId { get; set; }

    [Key(9)] public int AssetVariationId { get; set; }

    [Key(10)] public int MaterialSaleObtainPossessionId { get; set; }
}
