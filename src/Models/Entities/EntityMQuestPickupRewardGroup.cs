using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestPickupRewardGroup
{
    public int QuestPickupRewardGroupId { get; set; }

    public int SortOrder { get; set; }

    public int BattleDropRewardId { get; set; }
}
