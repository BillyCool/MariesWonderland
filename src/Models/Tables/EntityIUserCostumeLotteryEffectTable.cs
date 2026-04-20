using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCostumeLotteryEffectTable : TableBase<EntityIUserCostumeLotteryEffect>
{
    private readonly Func<EntityIUserCostumeLotteryEffect, (long, string, int)> primaryIndexSelector;

    public EntityIUserCostumeLotteryEffectTable(EntityIUserCostumeLotteryEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserCostumeUuid, element.SlotNumber);
    }

    public bool TryFindByUserIdAndUserCostumeUuidAndSlotNumber(ValueTuple<long, string, int> key, out EntityIUserCostumeLotteryEffect result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, int)>.Default, key, out result);

    public RangeView<EntityIUserCostumeLotteryEffect> FindRangeByUserIdAndUserCostumeUuidAndSlotNumber(ValueTuple<long, string, int> min, ValueTuple<long, string, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(long, string, int)>.Default, min, max, ascendant);
}
