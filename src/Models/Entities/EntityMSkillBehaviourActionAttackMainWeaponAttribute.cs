using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionAttackMainWeaponAttribute
{
    public int SkillBehaviourActionId { get; set; }

    public int SkillPower { get; set; }

    public AttributeType AttributeType { get; set; }

    public int MagnificationRate { get; set; }
}
