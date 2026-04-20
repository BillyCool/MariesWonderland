using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusWeaponGroupTable : TableBase<EntityMQuestBonusWeaponGroup>
{
    private readonly Func<EntityMQuestBonusWeaponGroup, (int, int, int)> primaryIndexSelector;

    public EntityMQuestBonusWeaponGroupTable(EntityMQuestBonusWeaponGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestBonusWeaponGroupId, element.WeaponId, element.LimitBreakCountLowerLimit);
    }

    public RangeView<EntityMQuestBonusWeaponGroup> FindRangeByQuestBonusWeaponGroupIdAndWeaponIdAndLimitBreakCountLowerLimit(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
