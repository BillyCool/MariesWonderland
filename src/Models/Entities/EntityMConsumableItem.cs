using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMConsumableItem))]
public class EntityMConsumableItem
{
    [Key(0)] public int ConsumableItemId { get; set; }

    [Key(1)] public ConsumableItemType ConsumableItemType { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public int SellPrice { get; set; }

    [Key(4)] public int ConsumableItemTermId { get; set; }

    [Key(5)] public string AssetName { get; set; }

    [Key(6)] public int AssetCategoryId { get; set; }

    [Key(7)] public int AssetVariationId { get; set; }
}
