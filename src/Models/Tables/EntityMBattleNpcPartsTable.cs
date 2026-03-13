using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcPartsTable : TableBase<EntityMBattleNpcParts>
{
    private readonly Func<EntityMBattleNpcParts, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcPartsTable(EntityMBattleNpcParts[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcPartsUuid);
    }

    public EntityMBattleNpcParts FindByBattleNpcIdAndBattleNpcPartsUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
