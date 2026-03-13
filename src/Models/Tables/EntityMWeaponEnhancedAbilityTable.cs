using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponEnhancedAbilityTable : TableBase<EntityMWeaponEnhancedAbility>
{
    private readonly Func<EntityMWeaponEnhancedAbility, (int, int)> primaryIndexSelector;

    public EntityMWeaponEnhancedAbilityTable(EntityMWeaponEnhancedAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponEnhancedId, element.AbilityId);
    }

    public bool TryFindByWeaponEnhancedIdAndAbilityId(ValueTuple<int, int> key, out EntityMWeaponEnhancedAbility result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
