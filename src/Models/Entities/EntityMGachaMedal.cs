using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMGachaMedal
{
    public int GachaMedalId { get; set; }

    public int CeilingCount { get; set; }

    public int ConsumableItemId { get; set; }

    public int ShopTransitionGachaId { get; set; }

    public long AutoConvertDatetime { get; set; }

    public int ConversionRate { get; set; }
}
