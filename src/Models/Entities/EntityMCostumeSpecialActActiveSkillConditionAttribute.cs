using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeSpecialActActiveSkillConditionAttribute))]
[PrimaryKey(nameof(CostumeSpecialActActiveSkillConditionId))]
public class EntityMCostumeSpecialActActiveSkillConditionAttribute
{
    [Key(0)] public int CostumeSpecialActActiveSkillConditionId { get; set; }

    [Key(1)] public AttributeType CostumeSpecialActActiveSkillConditionAttributeType { get; set; }
}
