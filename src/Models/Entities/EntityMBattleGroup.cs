using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleGroup))]
public class EntityMBattleGroup
{
    [Key(0)] public int BattleGroupId { get; set; }

    [Key(1)] public int WaveNumber { get; set; }

    [Key(2)] public int BattleId { get; set; }

    [Key(3)] public int WaveStartActAssetId { get; set; }

    [Key(4)] public int WaveEndActAssetId { get; set; }

    [Key(5)] public int BattleCameraControllerAssetId { get; set; }

    [Key(6)] public int BattlePointIndex { get; set; }

    [Key(7)] public BattleStartCameraType BattleStartCameraType { get; set; }
}
