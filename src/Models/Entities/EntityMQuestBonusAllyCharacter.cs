using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusAllyCharacter))]
[PrimaryKey(nameof(QuestBonusAllyCharacterId))]
public class EntityMQuestBonusAllyCharacter
{
    [Key(0)] public int QuestBonusAllyCharacterId { get; set; }

    [Key(1)] public int QuestBonusEffectGroupId { get; set; }

    [Key(2)] public int QuestBonusTermGroupId { get; set; }
}
