using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterVoiceUnlockCondition))]
[PrimaryKey(nameof(CharacterId), nameof(SortOrder))]
public class EntityMCharacterVoiceUnlockCondition
{
    [Key(0)] public int CharacterId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public CharacterVoiceUnlockConditionType CharacterVoiceUnlockConditionType { get; set; }

    [Key(3)] public int ConditionValue { get; set; }

    [Key(4)] public int VoiceAssetId { get; set; }
}
