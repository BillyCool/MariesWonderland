using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffectTargetAbility))]
[PrimaryKey(nameof(CostumeLotteryEffectTargetAbilityId))]
public class EntityMCostumeLotteryEffectTargetAbility
{
    [Key(0)] public int CostumeLotteryEffectTargetAbilityId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int AbilityLevel { get; set; }
}
