using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponSkillEnhancementMaterialTable : TableBase<EntityMWeaponSkillEnhancementMaterial>
{
    private readonly Func<EntityMWeaponSkillEnhancementMaterial, (int, int, int)> primaryIndexSelector;

    public EntityMWeaponSkillEnhancementMaterialTable(EntityMWeaponSkillEnhancementMaterial[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponSkillEnhancementMaterialId, element.SkillLevel, element.MaterialId);
    }

    public RangeView<EntityMWeaponSkillEnhancementMaterial> FindRangeByWeaponSkillEnhancementMaterialIdAndSkillLevelAndMaterialId(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
