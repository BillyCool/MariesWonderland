using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShop
{
    public int ShopId { get; set; }

    public ShopGroupType ShopGroupType { get; set; }

    public int SortOrderInShopGroup { get; set; }

    public ShopType ShopType { get; set; }

    public int NameShopTextId { get; set; }

    public ShopUpdatableLabelType ShopUpdatableLabelType { get; set; }

    public ShopExchangeType ShopExchangeType { get; set; }

    public int ShopItemCellGroupId { get; set; }

    public MainFunctionType RelatedMainFunctionType { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int LimitedOpenId { get; set; }
}
