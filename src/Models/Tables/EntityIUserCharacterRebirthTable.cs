using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCharacterRebirthTable : TableBase<EntityIUserCharacterRebirth>
{
    private readonly Func<EntityIUserCharacterRebirth, (long, int)> primaryIndexSelector;

    public EntityIUserCharacterRebirthTable(EntityIUserCharacterRebirth[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.CharacterId);
    }

    public EntityIUserCharacterRebirth FindByUserIdAndCharacterId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
