using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWebviewMissionTable : TableBase<EntityMWebviewMission>
{
    private readonly Func<EntityMWebviewMission, int> primaryIndexSelector;

    public EntityMWebviewMissionTable(EntityMWebviewMission[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WebviewMissionId;
    }
}
