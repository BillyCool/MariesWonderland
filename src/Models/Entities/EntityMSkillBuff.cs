using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBuff
{
    public int SkillBuffId { get; set; }

    public string BuffKey { get; set; }

    public BuffType BuffType { get; set; }

    public int Power { get; set; }

    public int CooltimeFrameCount { get; set; }

    public int IconId { get; set; }
}
