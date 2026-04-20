using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDeckCharacterTable : TableBase<EntityIUserDeckCharacter>
{
    private readonly Func<EntityIUserDeckCharacter, (long, string)> primaryIndexSelector;

    public EntityIUserDeckCharacterTable(EntityIUserDeckCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserDeckCharacterUuid);
    }

    public EntityIUserDeckCharacter FindByUserIdAndUserDeckCharacterUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);

    public bool TryFindByUserIdAndUserDeckCharacterUuid(ValueTuple<long, string> key, out EntityIUserDeckCharacter result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
