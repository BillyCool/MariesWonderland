using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestSequence))]
public class EntityMEventQuestSequence
{
    [Key(0)] public int EventQuestSequenceId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int QuestId { get; set; }
}
