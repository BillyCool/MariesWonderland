using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterViewerField
{
    public int CharacterViewerFieldId { get; set; }

    public int ReleaseEvaluateConditionId { get; set; }

    public long PublishDatetime { get; set; }

    public int CharacterViewerFieldAssetId { get; set; }

    public int AssetBackgroundId { get; set; }

    public int SortOrder { get; set; }
}
