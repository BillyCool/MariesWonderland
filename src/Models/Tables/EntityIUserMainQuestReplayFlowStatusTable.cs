using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMainQuestReplayFlowStatusTable : TableBase<EntityIUserMainQuestReplayFlowStatus>
{
    private readonly Func<EntityIUserMainQuestReplayFlowStatus, long> primaryIndexSelector;

    public EntityIUserMainQuestReplayFlowStatusTable(EntityIUserMainQuestReplayFlowStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserMainQuestReplayFlowStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
