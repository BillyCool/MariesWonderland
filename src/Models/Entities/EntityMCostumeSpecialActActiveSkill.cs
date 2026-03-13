using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeSpecialActActiveSkill
{
    public int CostumeId { get; set; }

    public int SkillActIndex { get; set; }

    public CostumeSpecialActActiveSkillConditionType CostumeSpecialActActiveSkillConditionType { get; set; }

    public int CostumeSpecialActActiveSkillConditionId { get; set; }
}
