using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMExploreGradeAssetTable : TableBase<EntityMExploreGradeAsset>
{
    private readonly Func<EntityMExploreGradeAsset, int> primaryIndexSelector;

    public EntityMExploreGradeAssetTable(EntityMExploreGradeAsset[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ExploreGradeId;
    }

    public EntityMExploreGradeAsset FindByExploreGradeId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
