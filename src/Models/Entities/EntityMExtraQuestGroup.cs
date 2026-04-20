using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExtraQuestGroup))]
public class EntityMExtraQuestGroup
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int ExtraQuestIndex { get; set; }

    [Key(2)] public int ExtraQuestId { get; set; }
}
