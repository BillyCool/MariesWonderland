using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckCharacterTable : TableBase<EntityMBattleNpcDeckCharacter>
{
    private readonly Func<EntityMBattleNpcDeckCharacter, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcDeckCharacterTable(EntityMBattleNpcDeckCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcDeckCharacterUuid);
    }

    public EntityMBattleNpcDeckCharacter FindByBattleNpcIdAndBattleNpcDeckCharacterUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
