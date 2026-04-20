using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourGroup))]
public class EntityMSkillAbnormalBehaviourGroup
{
    [Key(0)] public int SkillAbnormalBehaviourGroupId { get; set; }

    [Key(1)] public int AbnormalBehaviourIndex { get; set; }

    [Key(2)] public int SkillAbnormalBehaviourId { get; set; }
}
