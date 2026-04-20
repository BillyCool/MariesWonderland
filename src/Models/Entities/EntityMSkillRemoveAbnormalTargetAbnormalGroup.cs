using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillRemoveAbnormalTargetAbnormalGroup))]
public class EntityMSkillRemoveAbnormalTargetAbnormalGroup
{
    [Key(0)] public int SkillRemoveAbnormalTargetAbnormalGroupId { get; set; }

    [Key(1)] public int SkillRemoveAbnormalTargetAbnormalGroupIndex { get; set; }

    [Key(2)] public int AbnormalTypeId { get; set; }
}
