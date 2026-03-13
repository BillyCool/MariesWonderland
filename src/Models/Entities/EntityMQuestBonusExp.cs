using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusExp
{
    public int QuestBonusEffectId { get; set; }

    public int ExpType { get; set; }

    public int BonusValuePermil { get; set; }
}
