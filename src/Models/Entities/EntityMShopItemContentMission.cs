using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopItemContentMission
{
    public int ShopItemId { get; set; }

    public int MissionGroupId { get; set; }

    public bool IsReevaluateOnGrant { get; set; }
}
