using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCostumeTable : TableBase<EntityIUserCostume>
{
    private readonly Func<EntityIUserCostume, (long, string)> primaryIndexSelector;

    public EntityIUserCostumeTable(EntityIUserCostume[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserCostumeUuid);
    }

    public EntityIUserCostume FindByUserIdAndUserCostumeUuid(ValueTuple<long, string> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);

    public bool TryFindByUserIdAndUserCostumeUuid(ValueTuple<long, string> key, out EntityIUserCostume result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key, out result);
}
