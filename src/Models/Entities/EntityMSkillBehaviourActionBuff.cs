using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionBuff))]
public class EntityMSkillBehaviourActionBuff
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int SkillBuffId { get; set; }
}
