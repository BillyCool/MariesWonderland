using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviour))]
public class EntityMSkillAbnormalBehaviour
{
    [Key(0)] public int SkillAbnormalBehaviourId { get; set; }

    [Key(1)] public AbnormalBehaviourActionType AbnormalBehaviourActionType { get; set; }

    [Key(2)] public AbnormalBehaviourActivationMethodType AbnormalBehaviourActivationMethodType { get; set; }

    [Key(3)] public AbnormalBehaviourDeactivationMethodType AbnormalBehaviourDeactivationMethodType { get; set; }

    [Key(4)] public int SkillAbnormalBehaviourActionId { get; set; }
}
