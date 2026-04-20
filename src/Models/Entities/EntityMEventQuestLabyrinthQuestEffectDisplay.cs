using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthQuestEffectDisplay))]
public class EntityMEventQuestLabyrinthQuestEffectDisplay
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public LabyrinthQuestEffectDescriptionType LabyrinthQuestEffectDescriptionType { get; set; }

    [Key(3)] public int EventQuestLabyrinthQuestEffectDescriptionId { get; set; }

    [Key(4)] public AttributeType EffectTargetWeaponAttributeType { get; set; }
}
