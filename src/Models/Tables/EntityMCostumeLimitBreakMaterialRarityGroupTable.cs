using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeLimitBreakMaterialRarityGroupTable : TableBase<EntityMCostumeLimitBreakMaterialRarityGroup>
{
    private readonly Func<EntityMCostumeLimitBreakMaterialRarityGroup, (int, int)> primaryIndexSelector;

    public EntityMCostumeLimitBreakMaterialRarityGroupTable(EntityMCostumeLimitBreakMaterialRarityGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeLimitBreakMaterialRarityGroupId, element.MaterialId);
    }
}
