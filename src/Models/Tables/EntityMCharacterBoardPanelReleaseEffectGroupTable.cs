using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardPanelReleaseEffectGroupTable : TableBase<EntityMCharacterBoardPanelReleaseEffectGroup>
{
    private readonly Func<EntityMCharacterBoardPanelReleaseEffectGroup, (int, int)> primaryIndexSelector;

    public EntityMCharacterBoardPanelReleaseEffectGroupTable(EntityMCharacterBoardPanelReleaseEffectGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardPanelReleaseEffectGroupId, element.SortOrder);
    }

    public EntityMCharacterBoardPanelReleaseEffectGroup FindByCharacterBoardPanelReleaseEffectGroupIdAndSortOrder(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
