using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleBgmSet))]
public class EntityMBattleBgmSet
{
    [Key(0)] public int BgmSetId { get; set; }

    [Key(1)] public int TrackNumber { get; set; }

    [Key(2)] public int BgmAssetId { get; set; }

    [Key(3)] public int Stem { get; set; }

    [Key(4)] public int StartWaveNumber { get; set; }
}
