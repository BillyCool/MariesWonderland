using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponAbilityEnhancementMaterialTable : TableBase<EntityMWeaponAbilityEnhancementMaterial>
{
    private readonly Func<EntityMWeaponAbilityEnhancementMaterial, (int, int, int)> primaryIndexSelector;

    public EntityMWeaponAbilityEnhancementMaterialTable(EntityMWeaponAbilityEnhancementMaterial[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponAbilityEnhancementMaterialId, element.AbilityLevel, element.MaterialId);
    }

    public RangeView<EntityMWeaponAbilityEnhancementMaterial> FindRangeByWeaponAbilityEnhancementMaterialIdAndAbilityLevelAndMaterialId(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
