using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopItem
{
    public int ShopItemId { get; set; }

    public int NameShopTextId { get; set; }

    public int DescriptionShopTextId { get; set; }

    public int ShopItemContentType { get; set; }

    public PriceType PriceType { get; set; }

    public int PriceId { get; set; }

    public int Price { get; set; }

    public int RegularPrice { get; set; }

    public ShopPromotionType ShopPromotionType { get; set; }

    public int ShopItemLimitedStockId { get; set; }

    public int AssetCategoryId { get; set; }

    public int AssetVariationId { get; set; }

    public ShopItemDecorationType ShopItemDecorationType { get; set; }
}
