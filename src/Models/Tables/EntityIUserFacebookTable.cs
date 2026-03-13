using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserFacebookTable : TableBase<EntityIUserFacebook>
{
    private readonly Func<EntityIUserFacebook, long> primaryIndexSelector;

    public EntityIUserFacebookTable(EntityIUserFacebook[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserFacebook FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
