using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPortalCageStatusTable : TableBase<EntityIUserPortalCageStatus>
{
    private readonly Func<EntityIUserPortalCageStatus, long> primaryIndexSelector;

    public EntityIUserPortalCageStatusTable(EntityIUserPortalCageStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserPortalCageStatus FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
