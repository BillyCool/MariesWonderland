using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterTable : TableBase<EntityMBattleNpcCharacter>
{
    private readonly Func<EntityMBattleNpcCharacter, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcCharacterTable(EntityMBattleNpcCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterId);
    }

    public EntityMBattleNpcCharacter FindByBattleNpcIdAndCharacterId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
