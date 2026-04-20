using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserQuestLimitContentStatusTable : TableBase<EntityIUserQuestLimitContentStatus>
{
    private readonly Func<EntityIUserQuestLimitContentStatus, (long, int)> primaryIndexSelector;
    private readonly Func<EntityIUserQuestLimitContentStatus, int> secondaryIndexSelector;

    public EntityIUserQuestLimitContentStatusTable(EntityIUserQuestLimitContentStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.QuestId);
        secondaryIndexSelector = element => element.EventQuestChapterId;
    }

    public EntityIUserQuestLimitContentStatus FindByUserIdAndQuestId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
