using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleEventGroupTable : TableBase<EntityMBattleEventGroup>
{
    private readonly Func<EntityMBattleEventGroup, (int, int)> primaryIndexSelector;

    public EntityMBattleEventGroupTable(EntityMBattleEventGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleEventGroupId, element.BattleEventId);
    }

    public RangeView<EntityMBattleEventGroup> FindRangeByBattleEventGroupIdAndBattleEventId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
