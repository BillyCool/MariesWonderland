using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthQuestEffectDescriptionFree))]
public class EntityMEventQuestLabyrinthQuestEffectDescriptionFree
{
    [Key(0)] public int EventQuestLabyrinthQuestEffectDescriptionId { get; set; }

    [Key(1)] public int AssetId { get; set; }
}
