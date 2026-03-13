using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestMissionRewardTable : TableBase<EntityMQuestMissionReward>
{
    private readonly Func<EntityMQuestMissionReward, int> primaryIndexSelector;

    public EntityMQuestMissionRewardTable(EntityMQuestMissionReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestMissionRewardId;
    }
}
