using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPvpSeasonGrouping))]
public class EntityMPvpSeasonGrouping
{
    [Key(0)] public int PvpSeasonGroupingId { get; set; }

    [Key(1)] public int GroupId { get; set; }

    [Key(2)] public int DivideWeight { get; set; }
}
