using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopItemUserLevelCondition
{
    public int ShopItemId { get; set; }

    public int UserLevelUpperLimit { get; set; }

    public int UserLevelLowerLimit { get; set; }

    public int ShopItemAdditionalContentId { get; set; }
}
