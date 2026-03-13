using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCostumeLotteryEffectStatusUpTable : TableBase<EntityIUserCostumeLotteryEffectStatusUp>
{
    private readonly Func<EntityIUserCostumeLotteryEffectStatusUp, (long, string, StatusCalculationType)> primaryIndexSelector;

    public EntityIUserCostumeLotteryEffectStatusUpTable(EntityIUserCostumeLotteryEffectStatusUp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserCostumeUuid, element.StatusCalculationType);
    }

    public bool TryFindByUserIdAndUserCostumeUuidAndStatusCalculationType(ValueTuple<long, string, StatusCalculationType> key, out EntityIUserCostumeLotteryEffectStatusUp result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, StatusCalculationType)>.Default, key, out result);
}
