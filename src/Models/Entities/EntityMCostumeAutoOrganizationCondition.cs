using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAutoOrganizationCondition))]
[PrimaryKey(nameof(CostumeId), nameof(CostumeAutoOrganizationConditionType))]
public class EntityMCostumeAutoOrganizationCondition
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public CostumeAutoOrganizationConditionType CostumeAutoOrganizationConditionType { get; set; }

    [Key(2)] public int TargetValue { get; set; }
}
