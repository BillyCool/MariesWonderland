using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusAbility))]
public class EntityMQuestBonusAbility
{
    [Key(0)] public int QuestBonusEffectId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int Level { get; set; }
}
