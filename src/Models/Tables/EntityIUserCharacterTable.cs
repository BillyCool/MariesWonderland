using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCharacterTable : TableBase<EntityIUserCharacter>
{
    private readonly Func<EntityIUserCharacter, (long, int)> primaryIndexSelector;

    public EntityIUserCharacterTable(EntityIUserCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.CharacterId);
    }

    public EntityIUserCharacter FindByUserIdAndCharacterId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndCharacterId(ValueTuple<long, int> key, out EntityIUserCharacter result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
