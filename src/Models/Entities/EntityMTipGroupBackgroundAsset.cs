using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipGroupBackgroundAsset))]
public class EntityMTipGroupBackgroundAsset
{
    [Key(0)] public int TipGroupId { get; set; }

    [Key(1)] public string BackgroundAssetName { get; set; }

    [Key(2)] public int DisplayConditionClearQuestId { get; set; }
}
