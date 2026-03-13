using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserEventQuestTowerAccumulationRewardTable : TableBase<EntityIUserEventQuestTowerAccumulationReward>
{
    private readonly Func<EntityIUserEventQuestTowerAccumulationReward, (long, int)> primaryIndexSelector;

    public EntityIUserEventQuestTowerAccumulationRewardTable(EntityIUserEventQuestTowerAccumulationReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.EventQuestChapterId);
    }

    public EntityIUserEventQuestTowerAccumulationReward FindByUserIdAndEventQuestChapterId(ValueTuple<long, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
