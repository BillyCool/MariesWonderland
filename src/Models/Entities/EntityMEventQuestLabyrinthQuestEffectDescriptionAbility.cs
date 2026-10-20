using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthQuestEffectDescriptionAbility))]
[PrimaryKey(nameof(EventQuestLabyrinthQuestEffectDescriptionId))]
public class EntityMEventQuestLabyrinthQuestEffectDescriptionAbility
{
    [Key(0)] public int EventQuestLabyrinthQuestEffectDescriptionId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int AbilityLevel { get; set; }
}
