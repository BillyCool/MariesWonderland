using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickAdditionalAssetTable : TableBase<EntityMGimmickAdditionalAsset>
{
    private readonly Func<EntityMGimmickAdditionalAsset, int> primaryIndexSelector;

    public EntityMGimmickAdditionalAssetTable(EntityMGimmickAdditionalAsset[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.GimmickId;
    }
}
