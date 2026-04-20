using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickGroupTable : TableBase<EntityMGimmickGroup>
{
    private readonly Func<EntityMGimmickGroup, (int, int)> primaryIndexSelector;

    public EntityMGimmickGroupTable(EntityMGimmickGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.GimmickGroupId, element.GroupIndex);
    }
}
