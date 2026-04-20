using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestReleaseConditionDeckPowerTable : TableBase<EntityMQuestReleaseConditionDeckPower>
{
    private readonly Func<EntityMQuestReleaseConditionDeckPower, int> primaryIndexSelector;

    public EntityMQuestReleaseConditionDeckPowerTable(EntityMQuestReleaseConditionDeckPower[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestReleaseConditionId;
    }

    public EntityMQuestReleaseConditionDeckPower FindByQuestReleaseConditionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
