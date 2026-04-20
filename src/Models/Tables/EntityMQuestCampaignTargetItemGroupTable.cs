using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestCampaignTargetItemGroupTable : TableBase<EntityMQuestCampaignTargetItemGroup>
{
    private readonly Func<EntityMQuestCampaignTargetItemGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestCampaignTargetItemGroupTable(EntityMQuestCampaignTargetItemGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestCampaignTargetItemGroupId, element.TargetIndex);
    }
}
