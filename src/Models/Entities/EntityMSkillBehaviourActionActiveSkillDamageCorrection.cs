using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionActiveSkillDamageCorrection))]
[PrimaryKey(nameof(SkillBehaviourActionId))]
public class EntityMSkillBehaviourActionActiveSkillDamageCorrection
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int CorrectionValuePermil { get; set; }
}
