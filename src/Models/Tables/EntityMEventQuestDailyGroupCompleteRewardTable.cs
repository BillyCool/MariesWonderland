using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestDailyGroupCompleteRewardTable : TableBase<EntityMEventQuestDailyGroupCompleteReward>
{
    private readonly Func<EntityMEventQuestDailyGroupCompleteReward, (int, int)> primaryIndexSelector;

    public EntityMEventQuestDailyGroupCompleteRewardTable(EntityMEventQuestDailyGroupCompleteReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EventQuestDailyGroupCompleteRewardId, element.SortOrder);
    }
}
