using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterVoiceUnlockCondition
{
    public int CharacterId { get; set; }

    public int SortOrder { get; set; }

    public CharacterVoiceUnlockConditionType CharacterVoiceUnlockConditionType { get; set; }

    public int ConditionValue { get; set; }

    public int VoiceAssetId { get; set; }
}
