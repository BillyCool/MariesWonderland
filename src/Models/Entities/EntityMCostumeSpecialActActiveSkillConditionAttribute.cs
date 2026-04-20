using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeSpecialActActiveSkillConditionAttribute))]
public class EntityMCostumeSpecialActActiveSkillConditionAttribute
{
    [Key(0)] public int CostumeSpecialActActiveSkillConditionId { get; set; }

    [Key(1)] public AttributeType CostumeSpecialActActiveSkillConditionAttributeType { get; set; }
}
