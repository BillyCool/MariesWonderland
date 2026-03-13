using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserProfileTable : TableBase<EntityIUserProfile>
{
    private readonly Func<EntityIUserProfile, long> primaryIndexSelector;

    public EntityIUserProfileTable(EntityIUserProfile[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserProfile FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
