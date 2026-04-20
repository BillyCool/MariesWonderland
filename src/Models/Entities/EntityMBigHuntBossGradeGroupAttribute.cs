using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntBossGradeGroupAttribute))]
public class EntityMBigHuntBossGradeGroupAttribute
{
    [Key(0)] public AttributeType AttributeType { get; set; }

    [Key(1)] public int BigHuntBossGradeGroupId { get; set; }
}
