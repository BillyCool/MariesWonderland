using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLevelBonus))]
[PrimaryKey(nameof(CostumeLevelBonusId), nameof(Level))]
public class EntityMCostumeLevelBonus
{
    [Key(0)] public int CostumeLevelBonusId { get; set; }

    [Key(1)] public int Level { get; set; }

    [Key(2)] public CostumeLevelBonusType CostumeLevelBonusType { get; set; }

    [Key(3)] public int EffectValue { get; set; }
}
