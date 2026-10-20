using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMainQuestSequence))]
[PrimaryKey(nameof(MainQuestSequenceId), nameof(SortOrder))]
public class EntityMMainQuestSequence
{
    [Key(0)] public int MainQuestSequenceId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int QuestId { get; set; }
}
