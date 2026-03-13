using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDeckPartsGroupTable : TableBase<EntityIUserDeckPartsGroup>
{
    private readonly Func<EntityIUserDeckPartsGroup, (long, string, string)> primaryIndexSelector;

    public EntityIUserDeckPartsGroupTable(EntityIUserDeckPartsGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserDeckCharacterUuid, element.UserPartsUuid);
    }

    public EntityIUserDeckPartsGroup FindByUserIdAndUserDeckCharacterUuidAndUserPartsUuid((long, string, string) key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, string)>.Default, key);
}
