using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCompanionTable : TableBase<EntityMBattleNpcCompanion>
{
    private readonly Func<EntityMBattleNpcCompanion, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcCompanionTable(EntityMBattleNpcCompanion[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcCompanionUuid);
    }

    public EntityMBattleNpcCompanion FindByBattleNpcIdAndBattleNpcCompanionUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
