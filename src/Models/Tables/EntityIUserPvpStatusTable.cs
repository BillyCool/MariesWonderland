using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPvpStatusTable : TableBase<EntityIUserPvpStatus>
{
    private readonly Func<EntityIUserPvpStatus, long> primaryIndexSelector;

    public EntityIUserPvpStatusTable(EntityIUserPvpStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserPvpStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
