using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShop))]
public class EntityMShop
{
    [Key(0)] public int ShopId { get; set; }

    [Key(1)] public ShopGroupType ShopGroupType { get; set; }

    [Key(2)] public int SortOrderInShopGroup { get; set; }

    [Key(3)] public ShopType ShopType { get; set; }

    [Key(4)] public int NameShopTextId { get; set; }

    [Key(5)] public ShopUpdatableLabelType ShopUpdatableLabelType { get; set; }

    [Key(6)] public ShopExchangeType ShopExchangeType { get; set; }

    [Key(7)] public int ShopItemCellGroupId { get; set; }

    [Key(8)] public MainFunctionType RelatedMainFunctionType { get; set; }

    [Key(9)] public long StartDatetime { get; set; }

    [Key(10)] public long EndDatetime { get; set; }

    [Key(11)] public int LimitedOpenId { get; set; }
}
