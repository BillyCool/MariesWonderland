using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponStoryReleaseConditionGroup))]
public class EntityMWeaponStoryReleaseConditionGroup
{
    [Key(0)] public int WeaponStoryReleaseConditionGroupId { get; set; }

    [Key(1)] public int StoryIndex { get; set; }

    [Key(2)] public WeaponStoryReleaseConditionType WeaponStoryReleaseConditionType { get; set; }

    [Key(3)] public int ConditionValue { get; set; }

    [Key(4)] public int WeaponStoryReleaseConditionOperationGroupId { get; set; }
}
