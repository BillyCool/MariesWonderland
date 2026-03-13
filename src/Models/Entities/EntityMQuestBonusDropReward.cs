using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusDropReward
{
    public int QuestBonusEffectId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int AdditionalCount { get; set; }
}
