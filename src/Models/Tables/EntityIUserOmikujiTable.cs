using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserOmikujiTable : TableBase<EntityIUserOmikuji>
{
    private readonly Func<EntityIUserOmikuji, (long, int)> primaryIndexSelector;

    public EntityIUserOmikujiTable(EntityIUserOmikuji[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.OmikujiId);
    }

    public EntityIUserOmikuji FindByUserIdAndOmikujiId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);
}
