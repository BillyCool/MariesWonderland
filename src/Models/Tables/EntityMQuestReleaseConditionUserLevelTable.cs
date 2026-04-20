using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestReleaseConditionUserLevelTable : TableBase<EntityMQuestReleaseConditionUserLevel>
{
    private readonly Func<EntityMQuestReleaseConditionUserLevel, int> primaryIndexSelector;

    public EntityMQuestReleaseConditionUserLevelTable(EntityMQuestReleaseConditionUserLevel[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestReleaseConditionId;
    }

    public EntityMQuestReleaseConditionUserLevel FindByQuestReleaseConditionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
