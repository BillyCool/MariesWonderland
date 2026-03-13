using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckCharacterTypeTable : TableBase<EntityMBattleNpcDeckCharacterType>
{
    private readonly Func<EntityMBattleNpcDeckCharacterType, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcDeckCharacterTypeTable(EntityMBattleNpcDeckCharacterType[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcDeckCharacterUuid);
    }

    public EntityMBattleNpcDeckCharacterType FindByBattleNpcIdAndBattleNpcDeckCharacterUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
