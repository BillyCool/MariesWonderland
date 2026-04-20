using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionAbnormal))]
public class EntityMSkillBehaviourActionAbnormal
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int SkillBehaviourAbnormalId { get; set; }
}
