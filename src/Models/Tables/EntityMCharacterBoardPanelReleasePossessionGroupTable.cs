using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardPanelReleasePossessionGroupTable : TableBase<EntityMCharacterBoardPanelReleasePossessionGroup>
{
    private readonly Func<EntityMCharacterBoardPanelReleasePossessionGroup, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMCharacterBoardPanelReleasePossessionGroupTable(EntityMCharacterBoardPanelReleasePossessionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardPanelReleasePossessionGroupId, element.PossessionType, element.PossessionId);
    }

    public RangeView<EntityMCharacterBoardPanelReleasePossessionGroup> FindRangeByCharacterBoardPanelReleasePossessionGroupIdAndPossessionTypeAndPossessionId(
        ValueTuple<int, PossessionType, int> min, ValueTuple<int, PossessionType, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, PossessionType, int)>.Default, min, max, ascendant);
}
