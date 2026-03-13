using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestBonusWeaponGroup
{
    public int QuestBonusWeaponGroupId { get; set; }

    public int WeaponId { get; set; }

    public int LimitBreakCountLowerLimit { get; set; }

    public int QuestBonusEffectGroupId { get; set; }

    public int QuestBonusTermGroupId { get; set; }
}
