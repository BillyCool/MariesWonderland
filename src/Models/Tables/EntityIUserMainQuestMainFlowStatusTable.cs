using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserMainQuestMainFlowStatusTable : TableBase<EntityIUserMainQuestMainFlowStatus>
{
    private readonly Func<EntityIUserMainQuestMainFlowStatus, long> primaryIndexSelector;

    public EntityIUserMainQuestMainFlowStatusTable(EntityIUserMainQuestMainFlowStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserMainQuestMainFlowStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
