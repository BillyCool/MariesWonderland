using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionShortenActiveSkillCooltime))]
public class EntityMSkillBehaviourActionShortenActiveSkillCooltime
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public ActiveSkillType ActiveSkillType { get; set; }

    [Key(2)] public int ShortenValuePermil { get; set; }
}
