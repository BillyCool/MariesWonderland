using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserTripleDeckTable : TableBase<EntityIUserTripleDeck>
{
    private readonly Func<EntityIUserTripleDeck, (long, DeckType, int)> primaryIndexSelector;

    public EntityIUserTripleDeckTable(EntityIUserTripleDeck[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.DeckType, element.UserDeckNumber);
    }

    public EntityIUserTripleDeck FindByUserIdAndDeckTypeAndUserDeckNumber(ValueTuple<long, DeckType, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, DeckType, int)>.Default, key);

    public bool TryFindByUserIdAndDeckTypeAndUserDeckNumber(ValueTuple<long, DeckType, int> key, out EntityIUserTripleDeck result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, DeckType, int)>.Default, key, out result);
}
