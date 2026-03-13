using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserThoughtTable : TableBase<EntityIUserThought>
{
    private readonly Func<EntityIUserThought, (long, string)> primaryIndexSelector;

    public EntityIUserThoughtTable(EntityIUserThought[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserThoughtUuid);
    }

    public bool TryFindByUserIdAndUserThoughtUuid(ValueTuple<long, string> key, out EntityIUserThought result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
