using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPvpBackground
{
    public int AssetBackgroundId { get; set; }

    public int BattleFieldLocaleSettingIndex { get; set; }

    public int BattlePointIndex { get; set; }

    public int RandomWeight { get; set; }

    public int PostProcessConfigurationIndex { get; set; }

    public int BattleCameraControllerAssetId { get; set; }

    public BattleStartCameraType BattleStartCameraType { get; set; }

    public int WaveStartActAssetId { get; set; }

    public int WaveEndActAssetId { get; set; }
}
