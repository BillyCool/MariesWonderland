using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleBgmSetGroupTable : TableBase<EntityMBattleBgmSetGroup>
{
    private readonly Func<EntityMBattleBgmSetGroup, (int, int)> primaryIndexSelector;

    public EntityMBattleBgmSetGroupTable(EntityMBattleBgmSetGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BgmSetGroupId, element.BgmSetGroupIndex);
    }

    public RangeView<EntityMBattleBgmSetGroup> FindRangeByBgmSetGroupIdAndBgmSetGroupIndex(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
