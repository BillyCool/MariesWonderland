using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestReleaseConditionBigHuntScoreTable : TableBase<EntityMQuestReleaseConditionBigHuntScore>
{
    private readonly Func<EntityMQuestReleaseConditionBigHuntScore, int> primaryIndexSelector;

    public EntityMQuestReleaseConditionBigHuntScoreTable(EntityMQuestReleaseConditionBigHuntScore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestReleaseConditionId;
    }

    public EntityMQuestReleaseConditionBigHuntScore FindByQuestReleaseConditionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
