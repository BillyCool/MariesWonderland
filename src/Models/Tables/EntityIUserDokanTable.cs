using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserDokanTable : TableBase<EntityIUserDokan>
{
    private readonly Func<EntityIUserDokan, (long, int)> primaryIndexSelector;

    public EntityIUserDokanTable(EntityIUserDokan[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.DokanId);
    }

    public bool TryFindByUserIdAndDokanId(ValueTuple<long, int> key, out EntityIUserDokan result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
