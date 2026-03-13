using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionEnhancementMaterialTable : TableBase<EntityMCompanionEnhancementMaterial>
{
    private readonly Func<EntityMCompanionEnhancementMaterial, (int, int, int)> primaryIndexSelector;

    public EntityMCompanionEnhancementMaterialTable(EntityMCompanionEnhancementMaterial[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CompanionCategoryType, element.Level, element.MaterialId);
    }
}
