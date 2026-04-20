using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestRelationMainFlow))]
public class EntityMQuestRelationMainFlow
{
    [Key(0)] public int MainFlowQuestId { get; set; }

    [Key(1)] public DifficultyType DifficultyType { get; set; }

    [Key(2)] public int ReplayFlowQuestId { get; set; }

    [Key(3)] public int SubFlowQuestId { get; set; }
}
