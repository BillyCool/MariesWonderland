using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDefaultSkillLotteryGroup))]
[PrimaryKey(nameof(CostumeDefaultSkillLotteryGroupId), nameof(SkillDetailId))]
public class EntityMCostumeDefaultSkillLotteryGroup
{
    [Key(0)] public int CostumeDefaultSkillLotteryGroupId { get; set; }

    [Key(1)] public int SkillDetailId { get; set; }

    [Key(2)] public int ProbabilityWeight { get; set; }
}
