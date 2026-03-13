using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMConsumableItem
{
    public int ConsumableItemId { get; set; }

    public ConsumableItemType ConsumableItemType { get; set; }

    public int SortOrder { get; set; }

    public int SellPrice { get; set; }

    public int ConsumableItemTermId { get; set; }

    public string AssetName { get; set; }

    public int AssetCategoryId { get; set; }

    public int AssetVariationId { get; set; }
}
