using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeAwakenEffectGroupTable : TableBase<EntityMCostumeAwakenEffectGroup>
{
    private readonly Func<EntityMCostumeAwakenEffectGroup, (int, int, CostumeAwakenEffectType)> primaryIndexSelector;
    private readonly Func<EntityMCostumeAwakenEffectGroup, (int, CostumeAwakenEffectType)> secondaryIndexSelector;

    public EntityMCostumeAwakenEffectGroupTable(EntityMCostumeAwakenEffectGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CostumeAwakenEffectGroupId, element.AwakenStep, element.CostumeAwakenEffectType);
        secondaryIndexSelector = element => (element.CostumeAwakenEffectGroupId, element.CostumeAwakenEffectType);
    }

    public RangeView<EntityMCostumeAwakenEffectGroup> FindRangeByCostumeAwakenEffectGroupIdAndAwakenStepAndCostumeAwakenEffectType(ValueTuple<int, int, CostumeAwakenEffectType> min, ValueTuple<int, int, CostumeAwakenEffectType> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, CostumeAwakenEffectType)>.Default, min, max, ascendant);

    public RangeView<EntityMCostumeAwakenEffectGroup> FindByCostumeAwakenEffectGroupIdAndCostumeAwakenEffectType(ValueTuple<int, CostumeAwakenEffectType> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(int, CostumeAwakenEffectType)>.Default, key);

    public RangeView<EntityMCostumeAwakenEffectGroup> FindRangeByCostumeAwakenEffectGroupIdAndCostumeAwakenEffectType(ValueTuple<int, CostumeAwakenEffectType> min, ValueTuple<int, CostumeAwakenEffectType> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, secondaryIndexSelector, Comparer<(int, CostumeAwakenEffectType)>.Default, min, max, ascendant);
}
