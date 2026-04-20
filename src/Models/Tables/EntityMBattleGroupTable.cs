using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleGroupTable : TableBase<EntityMBattleGroup>
{
    private readonly Func<EntityMBattleGroup, (int, int)> primaryIndexSelector;

    public EntityMBattleGroupTable(EntityMBattleGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleGroupId, element.WaveNumber);
    }

    public RangeView<EntityMBattleGroup> FindRangeByBattleGroupIdAndWaveNumber(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
