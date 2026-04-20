using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterViewerField))]
public class EntityMCharacterViewerField
{
    [Key(0)] public int CharacterViewerFieldId { get; set; }

    [Key(1)] public int ReleaseEvaluateConditionId { get; set; }

    [Key(2)] public long PublishDatetime { get; set; }

    [Key(3)] public int CharacterViewerFieldAssetId { get; set; }

    [Key(4)] public int AssetBackgroundId { get; set; }

    [Key(5)] public int SortOrder { get; set; }
}
