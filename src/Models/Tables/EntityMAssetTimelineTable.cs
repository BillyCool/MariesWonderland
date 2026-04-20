using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAssetTimelineTable : TableBase<EntityMAssetTimeline>
{
    private readonly Func<EntityMAssetTimeline, int> primaryIndexSelector;

    public EntityMAssetTimelineTable(EntityMAssetTimeline[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AssetTimelineId;
    }

    public EntityMAssetTimeline FindByAssetTimelineId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
