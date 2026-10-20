using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDefaultSkillGroup))]
[PrimaryKey(nameof(CostumeDefaultSkillGroupId), nameof(CostumeDefaultSkillLotteryType))]
public class EntityMCostumeDefaultSkillGroup
{
    [Key(0)] public int CostumeDefaultSkillGroupId { get; set; }

    [Key(1)] public CostumeDefaultSkillLotteryType CostumeDefaultSkillLotteryType { get; set; }

    [Key(2)] public int CostumeDefaultSkillLotteryGroupId { get; set; }
}
