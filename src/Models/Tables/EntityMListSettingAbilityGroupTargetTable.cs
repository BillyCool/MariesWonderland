using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMListSettingAbilityGroupTargetTable : TableBase<EntityMListSettingAbilityGroupTarget>
{
    private readonly Func<EntityMListSettingAbilityGroupTarget, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMListSettingAbilityGroupTarget, int> secondaryIndexSelector;

    public EntityMListSettingAbilityGroupTargetTable(EntityMListSettingAbilityGroupTarget[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.ListSettingAbilityGroupTargetId, element.SortOrder);
        secondaryIndexSelector = element => element.ListSettingAbilityGroupTargetId;
    }

    public RangeView<EntityMListSettingAbilityGroupTarget> FindByListSettingAbilityGroupTargetId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
