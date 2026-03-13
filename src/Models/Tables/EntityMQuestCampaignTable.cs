using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestCampaignTable : TableBase<EntityMQuestCampaign>
{
    private readonly Func<EntityMQuestCampaign, int> primaryIndexSelector;

    public EntityMQuestCampaignTable(EntityMQuestCampaign[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestCampaignId;
    }
}
