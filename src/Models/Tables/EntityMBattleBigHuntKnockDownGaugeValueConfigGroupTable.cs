using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleBigHuntKnockDownGaugeValueConfigGroupTable : TableBase<EntityMBattleBigHuntKnockDownGaugeValueConfigGroup>
{
    private readonly Func<EntityMBattleBigHuntKnockDownGaugeValueConfigGroup, (int, int, int)> primaryIndexSelector;

    public EntityMBattleBigHuntKnockDownGaugeValueConfigGroupTable(EntityMBattleBigHuntKnockDownGaugeValueConfigGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.KnockDownGaugeValueConfigGroupId, element.ActiveSkillHitCount, element.DamageValueLowerLimit);
    }

    public RangeView<EntityMBattleBigHuntKnockDownGaugeValueConfigGroup> FindRangeByKnockDownGaugeValueConfigGroupIdAndActiveSkillHitCountAndDamageValueLowerLimit(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
