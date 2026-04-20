using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusCostumeSettingGroupTable : TableBase<EntityMQuestBonusCostumeSettingGroup>
{
    private readonly Func<EntityMQuestBonusCostumeSettingGroup, (int, int, int)> primaryIndexSelector;

    public EntityMQuestBonusCostumeSettingGroupTable(EntityMQuestBonusCostumeSettingGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestBonusCostumeSettingGroupId, element.CostumeId, element.LimitBreakCountLowerLimit);
    }

    public RangeView<EntityMQuestBonusCostumeSettingGroup> FindRangeByQuestBonusCostumeSettingGroupIdAndCostumeIdAndLimitBreakCountLowerLimit(ValueTuple<int, int, int> min, ValueTuple<int, int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int, int)>.Default, min, max, ascendant);
}
