using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleGroup
{
    public int BattleGroupId { get; set; }

    public int WaveNumber { get; set; }

    public int BattleId { get; set; }

    public int WaveStartActAssetId { get; set; }

    public int WaveEndActAssetId { get; set; }

    public int BattleCameraControllerAssetId { get; set; }

    public int BattlePointIndex { get; set; }

    public BattleStartCameraType BattleStartCameraType { get; set; }
}
