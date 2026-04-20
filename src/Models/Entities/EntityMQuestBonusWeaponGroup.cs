using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusWeaponGroup))]
public class EntityMQuestBonusWeaponGroup
{
    [Key(0)] public int QuestBonusWeaponGroupId { get; set; }

    [Key(1)] public int WeaponId { get; set; }

    [Key(2)] public int LimitBreakCountLowerLimit { get; set; }

    [Key(3)] public int QuestBonusEffectGroupId { get; set; }

    [Key(4)] public int QuestBonusTermGroupId { get; set; }
}
