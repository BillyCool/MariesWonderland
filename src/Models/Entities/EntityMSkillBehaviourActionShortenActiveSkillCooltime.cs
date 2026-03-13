using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionShortenActiveSkillCooltime
{
    public int SkillBehaviourActionId { get; set; }

    public ActiveSkillType ActiveSkillType { get; set; }

    public int ShortenValuePermil { get; set; }
}
