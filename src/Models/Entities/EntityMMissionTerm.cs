using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionTerm))]
[PrimaryKey(nameof(MissionTermId))]
public class EntityMMissionTerm
{
    [Key(0)] public int MissionTermId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }
}
