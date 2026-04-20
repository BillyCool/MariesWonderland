using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffectTargetStatusUp))]
public class EntityMCostumeLotteryEffectTargetStatusUp
{
    [Key(0)] public int CostumeLotteryEffectTargetStatusUpId { get; set; }

    [Key(1)] public StatusKindType StatusKindType { get; set; }

    [Key(2)] public StatusCalculationType StatusCalculationType { get; set; }

    [Key(3)] public int EffectValue { get; set; }
}
