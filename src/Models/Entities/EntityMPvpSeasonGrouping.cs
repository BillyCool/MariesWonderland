using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonGrouping))]
[PrimaryKey(nameof(PvpSeasonGroupingId), nameof(GroupId))]
public class EntityMPvpSeasonGrouping
{
    [Key(0)] public int PvpSeasonGroupingId { get; set; }

    [Key(1)] public int GroupId { get; set; }

    [Key(2)] public int DivideWeight { get; set; }
}
