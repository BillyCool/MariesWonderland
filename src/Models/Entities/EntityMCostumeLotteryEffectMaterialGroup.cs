using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffectMaterialGroup))]
[PrimaryKey(nameof(CostumeLotteryEffectMaterialGroupId), nameof(MaterialId))]
public class EntityMCostumeLotteryEffectMaterialGroup
{
    [Key(0)] public int CostumeLotteryEffectMaterialGroupId { get; set; }

    [Key(1)] public int MaterialId { get; set; }

    [Key(2)] public int Count { get; set; }

    [Key(3)] public int SortOrder { get; set; }
}
