using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionCharacterLevel))]
[PrimaryKey(nameof(QuestReleaseConditionId))]
public class EntityMQuestReleaseConditionCharacterLevel
{
    [Key(0)] public int QuestReleaseConditionId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public int CharacterLevel { get; set; }
}
