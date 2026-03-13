using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestTowerRewardGroupTable : TableBase<EntityMEventQuestTowerRewardGroup>
{
    private readonly Func<EntityMEventQuestTowerRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMEventQuestTowerRewardGroupTable(EntityMEventQuestTowerRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestTowerRewardGroupId, element.SortOrder);
    }

    public EntityMEventQuestTowerRewardGroup FindByEventQuestTowerRewardGroupIdAndSortOrder(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
