using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickIntervalTable : TableBase<EntityMGimmickInterval>
{
    private readonly Func<EntityMGimmickInterval, int> primaryIndexSelector;

    public EntityMGimmickIntervalTable(EntityMGimmickInterval[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.GimmickId;
    }
}
