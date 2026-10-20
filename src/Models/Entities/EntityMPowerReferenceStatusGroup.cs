using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPowerReferenceStatusGroup))]
[PrimaryKey(nameof(PowerReferenceStatusGroupId), nameof(ReferenceStatusType))]
public class EntityMPowerReferenceStatusGroup
{
    [Key(0)] public int PowerReferenceStatusGroupId { get; set; }

    [Key(1)] public StatusKindType ReferenceStatusType { get; set; }

    [Key(2)] public AttributeConditionType AttributeConditionType { get; set; }

    [Key(3)] public int CoefficientValuePermil { get; set; }
}
