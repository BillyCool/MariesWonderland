using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDeckLimitContentDeletedCharacterTable : TableBase<EntityIUserDeckLimitContentDeletedCharacter>
{
    private readonly Func<EntityIUserDeckLimitContentDeletedCharacter, (long, int, int)> primaryIndexSelector;
    private readonly Func<EntityIUserDeckLimitContentDeletedCharacter, (long, int)> secondaryIndexSelector;

    public EntityIUserDeckLimitContentDeletedCharacterTable(EntityIUserDeckLimitContentDeletedCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserDeckNumber, element.UserDeckCharacterNumber);
        secondaryIndexSelector = element => (element.UserId, element.UserDeckNumber);
    }

    public RangeView<EntityIUserDeckLimitContentDeletedCharacter> FindByUserIdAndUserDeckNumber(ValueTuple<long, int> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(long, int)>.Default, key);
}
