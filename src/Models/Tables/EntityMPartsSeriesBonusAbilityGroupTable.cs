using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsSeriesBonusAbilityGroupTable : TableBase<EntityMPartsSeriesBonusAbilityGroup>
{
    private readonly Func<EntityMPartsSeriesBonusAbilityGroup, (int, int, int)> primaryIndexSelector;

    public EntityMPartsSeriesBonusAbilityGroupTable(EntityMPartsSeriesBonusAbilityGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PartsSeriesBonusAbilityGroupId, element.SetCount, element.AbilityId);
    }

    public RangeView<EntityMPartsSeriesBonusAbilityGroup> FindRangeByPartsSeriesBonusAbilityGroupIdAndSetCountAndAbilityId(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
