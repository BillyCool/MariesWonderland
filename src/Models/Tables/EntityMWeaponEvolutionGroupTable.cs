using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponEvolutionGroupTable : TableBase<EntityMWeaponEvolutionGroup>
{
    private readonly Func<EntityMWeaponEvolutionGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMWeaponEvolutionGroup, (int, int)> secondaryIndexSelector;

    public EntityMWeaponEvolutionGroupTable(EntityMWeaponEvolutionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponEvolutionGroupId, element.EvolutionOrder);
        secondaryIndexSelector = element => (element.WeaponEvolutionGroupId, element.WeaponId);
    }

    public EntityMWeaponEvolutionGroup FindByWeaponEvolutionGroupIdAndEvolutionOrder(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public EntityMWeaponEvolutionGroup FindClosestByWeaponEvolutionGroupIdAndEvolutionOrder(ValueTuple<int, int> key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, selectLower);

    public RangeView<EntityMWeaponEvolutionGroup> FindByWeaponEvolutionGroupId(int key)
    {
        var result = data
            .Where(x => x.WeaponEvolutionGroupId == key)
            .ToArray();

        return new RangeView<EntityMWeaponEvolutionGroup>(result, 0, result.Length, true);
    }

    public RangeView<EntityMWeaponEvolutionGroup> FindByWeaponId(int key)
    {
        var result = data
            .Where(x => x.WeaponId == key)
            .ToArray();

        return new RangeView<EntityMWeaponEvolutionGroup>(result, 0, result.Length, true);
    }
}
