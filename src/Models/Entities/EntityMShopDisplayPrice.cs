using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopDisplayPrice
{
    public PriceType PriceType { get; set; }

    public int PriceId { get; set; }

    public int SortOrder { get; set; }
}
