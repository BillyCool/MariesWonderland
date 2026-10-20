using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponStoryReleaseConditionOperationGroup))]
[PrimaryKey(nameof(WeaponStoryReleaseConditionOperationGroupId))]
public class EntityMWeaponStoryReleaseConditionOperationGroup
{
    [Key(0)] public int WeaponStoryReleaseConditionOperationGroupId { get; set; }

    [Key(1)] public ConditionOperationType ConditionOperationType { get; set; }
}
