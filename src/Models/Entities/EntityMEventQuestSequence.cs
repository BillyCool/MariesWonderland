using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestSequence))]
[PrimaryKey(nameof(EventQuestSequenceId), nameof(SortOrder))]
public class EntityMEventQuestSequence
{
    [Key(0)] public int EventQuestSequenceId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int QuestId { get; set; }
}
