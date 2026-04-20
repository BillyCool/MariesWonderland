using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBuff))]
public class EntityMSkillBuff
{
    [Key(0)] public int SkillBuffId { get; set; }

    [Key(1)] public string BuffKey { get; set; }

    [Key(2)] public BuffType BuffType { get; set; }

    [Key(3)] public int Power { get; set; }

    [Key(4)] public int CooltimeFrameCount { get; set; }

    [Key(5)] public int IconId { get; set; }
}
