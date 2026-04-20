using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDeckCharacterDressupCostumeTable : TableBase<EntityIUserDeckCharacterDressupCostume>
{
    private readonly Func<EntityIUserDeckCharacterDressupCostume, (long, string)> primaryIndexSelector;

    public EntityIUserDeckCharacterDressupCostumeTable(EntityIUserDeckCharacterDressupCostume[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserDeckCharacterUuid);
    }

    public bool TryFindByUserIdAndUserDeckCharacterUuid(ValueTuple<long, string> key, out EntityIUserDeckCharacterDressupCostume result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
