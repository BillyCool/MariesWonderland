using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeActiveSkillEnhancementMaterialTable : TableBase<EntityMCostumeActiveSkillEnhancementMaterial>
{
    private readonly Func<EntityMCostumeActiveSkillEnhancementMaterial, (int, int, int)> primaryIndexSelector;

    public EntityMCostumeActiveSkillEnhancementMaterialTable(EntityMCostumeActiveSkillEnhancementMaterial[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeActiveSkillEnhancementMaterialId, element.SkillLevel, element.MaterialId);
    }

    public RangeView<EntityMCostumeActiveSkillEnhancementMaterial> FindRangeByCostumeActiveSkillEnhancementMaterialIdAndSkillLevelAndMaterialId(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
