using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsRarity
{
    public RarityType RarityType { get; set; }

    public int PartsLevelUpRateGroupId { get; set; }

    public int PartsLevelUpPriceGroupId { get; set; }

    public int SellPriceNumericalFunctionId { get; set; }
}
