using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffectOddsGroup))]
public class EntityMCostumeLotteryEffectOddsGroup
{
    [Key(0)] public int CostumeLotteryEffectOddsGroupId { get; set; }

    [Key(1)] public int OddsNumber { get; set; }

    [Key(2)] public int Weight { get; set; }

    [Key(3)] public CostumeLotteryEffectType CostumeLotteryEffectType { get; set; }

    [Key(4)] public int CostumeLotteryEffectTargetId { get; set; }

    [Key(5)] public RarityType RarityType { get; set; }
}
