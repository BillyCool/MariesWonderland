using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAutoOrganizationCondition))]
public class EntityMCostumeAutoOrganizationCondition
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public CostumeAutoOrganizationConditionType CostumeAutoOrganizationConditionType { get; set; }

    [Key(2)] public int TargetValue { get; set; }
}
