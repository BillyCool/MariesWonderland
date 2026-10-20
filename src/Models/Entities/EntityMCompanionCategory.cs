using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionCategory))]
[PrimaryKey(nameof(CompanionCategoryType))]
public class EntityMCompanionCategory
{
    [Key(0)] public int CompanionCategoryType { get; set; }

    [Key(1)] public int EnhancementCostNumericalFunctionId { get; set; }
}
