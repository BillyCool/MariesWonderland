using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleBigHuntPhaseGroupTable : TableBase<EntityMBattleBigHuntPhaseGroup>
{
    private readonly Func<EntityMBattleBigHuntPhaseGroup, (int, int)> primaryIndexSelector;

    public EntityMBattleBigHuntPhaseGroupTable(EntityMBattleBigHuntPhaseGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleBigHuntPhaseGroupId, element.BattleBigHuntPhaseGroupOrder);
    }

    public RangeView<EntityMBattleBigHuntPhaseGroup> FindRangeByBattleBigHuntPhaseGroupIdAndBattleBigHuntPhaseGroupOrder(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
