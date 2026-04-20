using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionExtendBuffCooltime))]
public class EntityMSkillBehaviourActionExtendBuffCooltime
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public ExtendBuffCooltimeBuffType ExtendBuffCooltimeBuffType { get; set; }

    [Key(2)] public ExtendBuffCooltimeStatusType ExtendBuffCooltimeStatusType { get; set; }

    [Key(3)] public ExtendBuffCooltimeTargetSkillType ExtendBuffCooltimeTargetSkillType { get; set; }

    [Key(4)] public ExtendBuffCooltimeExtendType ExtendBuffCooltimeExtendType { get; set; }

    [Key(5)] public int ExtendValue { get; set; }
}
