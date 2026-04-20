using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionRemoveAbnormal))]
public class EntityMSkillBehaviourActionRemoveAbnormal
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int TargetPolarityType { get; set; }

    [Key(2)] public int SkillRemoveAbnormalTargetAbnormalGroupId { get; set; }

    [Key(3)] public RemoveAbnormalTargetType RemoveAbnormalTargetType { get; set; }

    [Key(4)] public int RemoveCountUpper { get; set; }

    [Key(5)] public int RemoveKindCountUpper { get; set; }
}
