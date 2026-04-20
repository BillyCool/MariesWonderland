using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterViewerFieldSettings))]
public class EntityMCharacterViewerFieldSettings
{
    [Key(0)] public int AssetBackgroundId { get; set; }

    [Key(1)] public int BgmAssetId { get; set; }

    [Key(2)] public int Stem { get; set; }

    [Key(3)] public int BattleFieldLocaleSettingIndex { get; set; }

    [Key(4)] public int PostProcessConfigurationIndex { get; set; }

    [Key(5)] public int BattlePointIndex { get; set; }
}
