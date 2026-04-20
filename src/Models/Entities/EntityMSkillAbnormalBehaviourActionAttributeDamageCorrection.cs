using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillAbnormalBehaviourActionAttributeDamageCorrection))]
public class EntityMSkillAbnormalBehaviourActionAttributeDamageCorrection
{
    [Key(0)] public int SkillAbnormalBehaviourActionId { get; set; }

    [Key(1)] public AttributeType AttributeType { get; set; }

    [Key(2)] public CorrectionTargetDamageType CorrectionTargetDamageType { get; set; }

    [Key(3)] public int CorrectionValuePermil { get; set; }

    [Key(4)] public DamageCorrectionOverlapType DamageCorrectionOverlapType { get; set; }

    [Key(5)] public bool IsExcepting { get; set; }
}
