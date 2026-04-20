using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItem))]
public class EntityMShopItem
{
    [Key(0)] public int ShopItemId { get; set; }

    [Key(1)] public int NameShopTextId { get; set; }

    [Key(2)] public int DescriptionShopTextId { get; set; }

    [Key(3)] public int ShopItemContentType { get; set; }

    [Key(4)] public PriceType PriceType { get; set; }

    [Key(5)] public int PriceId { get; set; }

    [Key(6)] public int Price { get; set; }

    [Key(7)] public int RegularPrice { get; set; }

    [Key(8)] public ShopPromotionType ShopPromotionType { get; set; }

    [Key(9)] public int ShopItemLimitedStockId { get; set; }

    [Key(10)] public int AssetCategoryId { get; set; }

    [Key(11)] public int AssetVariationId { get; set; }

    [Key(12)] public ShopItemDecorationType ShopItemDecorationType { get; set; }
}
