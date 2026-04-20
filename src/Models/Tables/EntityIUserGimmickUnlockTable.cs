using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserGimmickUnlockTable : TableBase<EntityIUserGimmickUnlock>
{
    private readonly Func<EntityIUserGimmickUnlock, (long, int, int, int)> primaryIndexSelector;

    public EntityIUserGimmickUnlockTable(EntityIUserGimmickUnlock[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.GimmickSequenceScheduleId, element.GimmickSequenceId, element.GimmickId);
    }

    public bool TryFindByUserIdAndGimmickSequenceScheduleIdAndGimmickSequenceIdAndGimmickId(ValueTuple<long, int, int, int> key, out EntityIUserGimmickUnlock result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int, int)>.Default, key, out result);
}
