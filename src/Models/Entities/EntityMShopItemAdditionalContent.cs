using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopItemAdditionalContent
{
    public int ShopItemAdditionalContentId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
