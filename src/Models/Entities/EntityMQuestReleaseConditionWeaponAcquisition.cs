using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionWeaponAcquisition))]
[PrimaryKey(nameof(QuestReleaseConditionId))]
public class EntityMQuestReleaseConditionWeaponAcquisition
{
    [Key(0)] public int QuestReleaseConditionId { get; set; }

    [Key(1)] public int WeaponId { get; set; }
}
