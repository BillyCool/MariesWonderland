using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickSequenceGroupTable : TableBase<EntityMGimmickSequenceGroup>
{
    private readonly Func<EntityMGimmickSequenceGroup, (int, int)> primaryIndexSelector;

    public EntityMGimmickSequenceGroupTable(EntityMGimmickSequenceGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.GimmickSequenceGroupId, element.GroupIndex);
    }
}
