using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterDisplaySwitch
{
    public int CharacterId { get; set; }

    public int NameCharacterTextId { get; set; }

    public int DefaultCostumeId { get; set; }

    public int DefaultWeaponId { get; set; }

    public int DisplayConditionClearQuestId { get; set; }

    public int CharacterAssetId { get; set; }
}
