using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponStoryReleaseConditionOperation))]
[PrimaryKey(nameof(WeaponStoryReleaseConditionOperationGroupId), nameof(SortOrder))]
public class EntityMWeaponStoryReleaseConditionOperation
{
    [Key(0)] public int WeaponStoryReleaseConditionOperationGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public WeaponStoryReleaseConditionType WeaponStoryReleaseConditionType { get; set; }

    [Key(3)] public int ConditionValue { get; set; }
}
