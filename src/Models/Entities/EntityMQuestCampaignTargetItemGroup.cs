using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestCampaignTargetItemGroup
{
    public int QuestCampaignTargetItemGroupId { get; set; }

    public int TargetIndex { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }
}
