using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusAllyCharacter))]
public class EntityMQuestBonusAllyCharacter
{
    [Key(0)] public int QuestBonusAllyCharacterId { get; set; }

    [Key(1)] public int QuestBonusEffectGroupId { get; set; }

    [Key(2)] public int QuestBonusTermGroupId { get; set; }
}
