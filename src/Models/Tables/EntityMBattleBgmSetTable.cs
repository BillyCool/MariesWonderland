using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleBgmSetTable : TableBase<EntityMBattleBgmSet>
{
    private readonly Func<EntityMBattleBgmSet, (int, int)> primaryIndexSelector;

    public EntityMBattleBgmSetTable(EntityMBattleBgmSet[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BgmSetId, element.TrackNumber);
    }

    public RangeView<EntityMBattleBgmSet> FindRangeByBgmSetIdAndTrackNumber(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
