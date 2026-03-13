using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPremiumItem
{
    public int PremiumItemId { get; set; }

    public int PremiumItemType { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
