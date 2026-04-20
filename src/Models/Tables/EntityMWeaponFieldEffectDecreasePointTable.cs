using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponFieldEffectDecreasePointTable : TableBase<EntityMWeaponFieldEffectDecreasePoint>
{
    private readonly Func<EntityMWeaponFieldEffectDecreasePoint, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMWeaponFieldEffectDecreasePoint, (int, bool)> secondaryIndexSelector;

    public EntityMWeaponFieldEffectDecreasePointTable(EntityMWeaponFieldEffectDecreasePoint[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponId, element.RelationIndex);
        secondaryIndexSelector = element => (element.WeaponId, element.IsWeaponAwaken);
    }

    public RangeView<EntityMWeaponFieldEffectDecreasePoint> FindRangeByWeaponIdAndRelationIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);

    public RangeView<EntityMWeaponFieldEffectDecreasePoint> FindRangeByWeaponIdAndIsWeaponAwaken(ValueTuple<int, bool> min, ValueTuple<int, bool> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, secondaryIndexSelector, Comparer<(int, bool)>.Default, min, max, ascendant);
}
