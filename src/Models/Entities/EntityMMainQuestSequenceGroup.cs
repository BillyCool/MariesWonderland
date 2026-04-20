using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMainQuestSequenceGroup))]
public class EntityMMainQuestSequenceGroup
{
    [Key(0)] public int MainQuestSequenceGroupId { get; set; }

    [Key(1)] public DifficultyType DifficultyType { get; set; }

    [Key(2)] public int MainQuestSequenceId { get; set; }
}
