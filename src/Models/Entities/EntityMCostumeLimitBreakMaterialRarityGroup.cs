using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLimitBreakMaterialRarityGroup))]
[PrimaryKey(nameof(CostumeLimitBreakMaterialRarityGroupId), nameof(MaterialId))]
public class EntityMCostumeLimitBreakMaterialRarityGroup
{
    [Key(0)] public int CostumeLimitBreakMaterialRarityGroupId { get; set; }

    [Key(1)] public int MaterialId { get; set; }

    [Key(2)] public int Count { get; set; }

    [Key(3)] public int SortOrder { get; set; }
}
