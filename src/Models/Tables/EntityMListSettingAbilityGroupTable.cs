using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMListSettingAbilityGroupTable : TableBase<EntityMListSettingAbilityGroup>
{
    private readonly Func<EntityMListSettingAbilityGroup, int> primaryIndexSelector;

    public EntityMListSettingAbilityGroupTable(EntityMListSettingAbilityGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ListSettingAbilityGroupId;
    }

    public EntityMListSettingAbilityGroup FindByListSettingAbilityGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
