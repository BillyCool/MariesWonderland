using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup))]
[PrimaryKey(nameof(SkillCooltimeAdvanceValueOnDefaultSkillGroupId), nameof(SkillHitCountLowerLimit))]
public class EntityMSkillCooltimeAdvanceValueOnDefaultSkillGroup
{
    [Key(0)] public int SkillCooltimeAdvanceValueOnDefaultSkillGroupId { get; set; }

    [Key(1)] public int SkillHitCountLowerLimit { get; set; }

    [Key(2)] public int SkillCooltimeAdvanceValue { get; set; }
}
