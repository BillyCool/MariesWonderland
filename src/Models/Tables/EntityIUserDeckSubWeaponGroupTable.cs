using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDeckSubWeaponGroupTable : TableBase<EntityIUserDeckSubWeaponGroup>
{
    private readonly Func<EntityIUserDeckSubWeaponGroup, (long, string, string)> primaryIndexSelector;

    public EntityIUserDeckSubWeaponGroupTable(EntityIUserDeckSubWeaponGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserDeckCharacterUuid, element.UserWeaponUuid);
    }

    public EntityIUserDeckSubWeaponGroup FindByUserIdAndUserDeckCharacterUuidAndUserWeaponUuid((long, string, string) key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, string)>.Default, key);
}
