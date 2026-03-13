using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeLimitBreakMaterialRarityGroup
{
    public int CostumeLimitBreakMaterialRarityGroupId { get; set; }

    public int MaterialId { get; set; }

    public int Count { get; set; }

    public int SortOrder { get; set; }
}
