using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntBossGradeGroupAttribute))]
[PrimaryKey(nameof(AttributeType))]
public class EntityMBigHuntBossGradeGroupAttribute
{
    [Key(0)] public AttributeType AttributeType { get; set; }

    [Key(1)] public int BigHuntBossGradeGroupId { get; set; }
}
