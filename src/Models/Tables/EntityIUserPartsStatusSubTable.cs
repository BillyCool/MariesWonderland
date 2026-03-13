using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPartsStatusSubTable : TableBase<EntityIUserPartsStatusSub>
{
    private readonly Func<EntityIUserPartsStatusSub, (long, string, int)> primaryIndexSelector;

    public EntityIUserPartsStatusSubTable(EntityIUserPartsStatusSub[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserPartsUuid, element.StatusIndex);
    }

    public EntityIUserPartsStatusSub FindByUserIdAndUserPartsUuidAndStatusIndex((long, string, int) key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, int)>.Default, key);
}
