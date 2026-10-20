using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCageOrnament))]
[PrimaryKey(nameof(CageOrnamentId))]
public class EntityMCageOrnament
{
    [Key(0)] public int CageOrnamentId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }

    [Key(3)] public int CageOrnamentRewardId { get; set; }
}
