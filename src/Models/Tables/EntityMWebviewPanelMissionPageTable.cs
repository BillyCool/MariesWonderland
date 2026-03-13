using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWebviewPanelMissionPageTable : TableBase<EntityMWebviewPanelMissionPage>
{
    private readonly Func<EntityMWebviewPanelMissionPage, int> primaryIndexSelector;

    public EntityMWebviewPanelMissionPageTable(EntityMWebviewPanelMissionPage[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.WebviewPanelMissionPageId;
    }

    public EntityMWebviewPanelMissionPage FindByWebviewPanelMissionPageId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
