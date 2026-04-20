using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMWebviewPanelMissionCompleteFlavorTextTable : TableBase<EntityMWebviewPanelMissionCompleteFlavorText>
{
    private readonly Func<EntityMWebviewPanelMissionCompleteFlavorText, (int, LanguageType)> primaryIndexSelector;

    public EntityMWebviewPanelMissionCompleteFlavorTextTable(EntityMWebviewPanelMissionCompleteFlavorText[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WebviewPanelMissionCompleteFlavorTextId, element.LanguageType);
    }

    public EntityMWebviewPanelMissionCompleteFlavorText FindByWebviewPanelMissionCompleteFlavorTextIdAndLanguageType(ValueTuple<int, LanguageType> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, LanguageType)>.Default, key);
}
