using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPortalCageSceneTable : TableBase<EntityMPortalCageScene>
{
    private readonly Func<EntityMPortalCageScene, int> primaryIndexSelector;

    public EntityMPortalCageSceneTable(EntityMPortalCageScene[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PortalCageSceneId;
    }
}
