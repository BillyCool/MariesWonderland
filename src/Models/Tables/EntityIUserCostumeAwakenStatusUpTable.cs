using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCostumeAwakenStatusUpTable : TableBase<EntityIUserCostumeAwakenStatusUp>
{
    private readonly Func<EntityIUserCostumeAwakenStatusUp, (long, string, StatusCalculationType)> primaryIndexSelector;

    public EntityIUserCostumeAwakenStatusUpTable(EntityIUserCostumeAwakenStatusUp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserCostumeUuid, element.StatusCalculationType);
    }

    public bool TryFindByUserIdAndUserCostumeUuidAndStatusCalculationType(ValueTuple<long, string, StatusCalculationType> key, out EntityIUserCostumeAwakenStatusUp result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, StatusCalculationType)>.Default, key, out result);
}
