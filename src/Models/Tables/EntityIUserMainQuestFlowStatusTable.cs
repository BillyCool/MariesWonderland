using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMainQuestFlowStatusTable : TableBase<EntityIUserMainQuestFlowStatus>
{
    private readonly Func<EntityIUserMainQuestFlowStatus, long> primaryIndexSelector;

    public EntityIUserMainQuestFlowStatusTable(EntityIUserMainQuestFlowStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserMainQuestFlowStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
