using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMNaviCutInContentGroupTable : TableBase<EntityMNaviCutInContentGroup>
{
    private readonly Func<EntityMNaviCutInContentGroup, (int, int)> primaryIndexSelector;

    public EntityMNaviCutInContentGroupTable(EntityMNaviCutInContentGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.NaviCutInContentGroupId, element.ContentIndex);
    }
}
