using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardEffectTargetGroupTable : TableBase<EntityMCharacterBoardEffectTargetGroup>
{
    private readonly Func<EntityMCharacterBoardEffectTargetGroup, (int, int)> primaryIndexSelector;

    public EntityMCharacterBoardEffectTargetGroupTable(EntityMCharacterBoardEffectTargetGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardEffectTargetGroupId, element.GroupIndex);
    }

    public RangeView<EntityMCharacterBoardEffectTargetGroup> FindRangeByCharacterBoardEffectTargetGroupIdAndGroupIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
