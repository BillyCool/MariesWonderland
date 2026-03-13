using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAssetDataSettingTable : TableBase<EntityMAssetDataSetting>
{
    private readonly Func<EntityMAssetDataSetting, int> primaryIndexSelector;

    public EntityMAssetDataSettingTable(EntityMAssetDataSetting[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AssetDataSettingId;
    }
}
