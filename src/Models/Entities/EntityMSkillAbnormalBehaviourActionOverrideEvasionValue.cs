using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionOverrideEvasionValue))]
public class EntityMSkillAbnormalBehaviourActionOverrideEvasionValue
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public int CorrectionValuePermil { get; set; }
}
