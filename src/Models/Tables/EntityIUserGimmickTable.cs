using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserGimmickTable : TableBase<EntityIUserGimmick>
{
    private readonly Func<EntityIUserGimmick, (long, int, int, int)> primaryIndexSelector;

    public EntityIUserGimmickTable(EntityIUserGimmick[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.GimmickSequenceScheduleId, element.GimmickSequenceId, element.GimmickId);
    }

    public EntityIUserGimmick FindByUserId((long, int, int, int) key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int, int)>.Default, key);
}
