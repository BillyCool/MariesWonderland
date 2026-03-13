using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMFieldEffectDecreasePointTable : TableBase<EntityMFieldEffectDecreasePoint>
{
    private readonly Func<EntityMFieldEffectDecreasePoint, (int, int)> primaryIndexSelector;

    public EntityMFieldEffectDecreasePointTable(EntityMFieldEffectDecreasePoint[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponId, element.FieldEffectAbilityId);
    }
}
