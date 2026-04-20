using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDefaultSkillLotteryGroup))]
public class EntityMCostumeDefaultSkillLotteryGroup
{
    [Key(0)] public int CostumeDefaultSkillLotteryGroupId { get; set; }

    [Key(1)] public int SkillDetailId { get; set; }

    [Key(2)] public int ProbabilityWeight { get; set; }
}
