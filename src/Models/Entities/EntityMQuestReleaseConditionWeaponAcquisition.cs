using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionWeaponAcquisition))]
public class EntityMQuestReleaseConditionWeaponAcquisition
{
    [Key(0)] public int QuestReleaseConditionId { get; set; }

    [Key(1)] public int WeaponId { get; set; }
}
