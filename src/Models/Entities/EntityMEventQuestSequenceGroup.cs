using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestSequenceGroup))]
[PrimaryKey(nameof(EventQuestSequenceGroupId), nameof(DifficultyType))]
public class EntityMEventQuestSequenceGroup
{
    // Properties
    [Key(0)] public int EventQuestSequenceGroupId { get; set; }

    [Key(1)] public DifficultyType DifficultyType { get; set; }

    [Key(2)] public int EventQuestSequenceId { get; set; }
}
