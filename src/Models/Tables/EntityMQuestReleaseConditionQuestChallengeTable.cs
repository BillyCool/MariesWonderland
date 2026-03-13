using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestReleaseConditionQuestChallengeTable : TableBase<EntityMQuestReleaseConditionQuestChallenge>
{
    private readonly Func<EntityMQuestReleaseConditionQuestChallenge, int> primaryIndexSelector;

    public EntityMQuestReleaseConditionQuestChallengeTable(EntityMQuestReleaseConditionQuestChallenge[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestReleaseConditionId;
    }

    public EntityMQuestReleaseConditionQuestChallenge FindByQuestReleaseConditionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
