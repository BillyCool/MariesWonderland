using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserWeaponStoryTable : TableBase<EntityIUserWeaponStory>
{
    private readonly Func<EntityIUserWeaponStory, (long, int)> primaryIndexSelector;

    public EntityIUserWeaponStoryTable(EntityIUserWeaponStory[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.WeaponId);
    }

    public EntityIUserWeaponStory FindByUserIdAndWeaponId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
