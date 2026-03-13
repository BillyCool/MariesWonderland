using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPartsTable : TableBase<EntityIUserParts>
{
    private readonly Func<EntityIUserParts, (long, string)> primaryIndexSelector;

    public EntityIUserPartsTable(EntityIUserParts[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserPartsUuid);
    }

    public EntityIUserParts FindByUserIdAndUserPartsUuid(ValueTuple<long, string> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);

    public bool TryFindByUserIdAndUserPartsUuid(ValueTuple<long, string> key, out EntityIUserParts result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
