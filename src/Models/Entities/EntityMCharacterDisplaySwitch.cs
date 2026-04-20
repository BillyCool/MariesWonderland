using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterDisplaySwitch))]
public class EntityMCharacterDisplaySwitch
{
    [Key(0)] public int CharacterId { get; set; }

    [Key(1)] public int NameCharacterTextId { get; set; }

    [Key(2)] public int DefaultCostumeId { get; set; }

    [Key(3)] public int DefaultWeaponId { get; set; }

    [Key(4)] public int DisplayConditionClearQuestId { get; set; }

    [Key(5)] public int CharacterAssetId { get; set; }
}
