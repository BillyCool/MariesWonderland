using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMComboCalculationSettingTable : TableBase<EntityMComboCalculationSetting>
{
    private readonly Func<EntityMComboCalculationSetting, int> primaryIndexSelector;

    public EntityMComboCalculationSettingTable(EntityMComboCalculationSetting[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ComboCountLowerLimit;
    }
}
