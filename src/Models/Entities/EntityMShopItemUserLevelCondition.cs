using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemUserLevelCondition))]
public class EntityMShopItemUserLevelCondition
{
    [Key(0)] public int ShopItemId { get; set; }

    [Key(1)] public int UserLevelUpperLimit { get; set; }

    [Key(2)] public int UserLevelLowerLimit { get; set; }

    [Key(3)] public int ShopItemAdditionalContentId { get; set; }
}
