using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionDefaultSkillLottery))]
public class EntityMSkillBehaviourActionDefaultSkillLottery
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int TargetCountLower { get; set; }

    [Key(2)] public int TargetCountUpper { get; set; }

    [Key(3)] public int ValuePermil { get; set; }

    [Key(4)] public int CalculationType { get; set; }
}
