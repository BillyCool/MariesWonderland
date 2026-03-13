using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeLimitBreakMaterialGroup
{
    public int CostumeLimitBreakMaterialGroupId { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public int SortOrder { get; set; }

    public int CostumeOverflowExchangePossessionGroupId { get; set; }
}
