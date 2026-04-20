using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAssetDataSetting))]
public class EntityMAssetDataSetting
{
    [Key(0)] public int AssetDataSettingId { get; set; }

    [Key(1)] public string AssetPath { get; set; }
}
