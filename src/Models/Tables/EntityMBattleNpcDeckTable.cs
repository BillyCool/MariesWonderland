using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckTable : TableBase<EntityMBattleNpcDeck>
{
    private readonly Func<EntityMBattleNpcDeck, (long, DeckType, int)> primaryIndexSelector;

    public EntityMBattleNpcDeckTable(EntityMBattleNpcDeck[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.DeckType, element.BattleNpcDeckNumber);
    }

    public EntityMBattleNpcDeck FindByBattleNpcIdAndDeckTypeAndBattleNpcDeckNumber(ValueTuple<long, DeckType, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, DeckType, int)>.Default, key);
}
