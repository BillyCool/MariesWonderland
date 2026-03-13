using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickGroupEventLogTable : TableBase<EntityMGimmickGroupEventLog>
{
    private readonly Func<EntityMGimmickGroupEventLog, int> primaryIndexSelector;

    public EntityMGimmickGroupEventLogTable(EntityMGimmickGroupEventLog[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.GimmickGroupId;
    }

    public bool TryFindByGimmickGroupId(int key, out EntityMGimmickGroupEventLog result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
