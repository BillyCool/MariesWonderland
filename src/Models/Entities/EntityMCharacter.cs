using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacter
{
    public int CharacterId { get; set; }

    public int CharacterLevelBonusAbilityGroupId { get; set; }

    public int NameCharacterTextId { get; set; }

    public int CharacterAssetId { get; set; }

    public int SortOrder { get; set; }

    public int DefaultCostumeId { get; set; }

    public int DefaultWeaponId { get; set; }

    public int EndCostumeId { get; set; }

    public int EndWeaponId { get; set; }

    public int MaxLevelNumericalFunctionId { get; set; }

    public int RequiredExpForLevelUpNumericalParameterMapId { get; set; }

    public int ListSettingCostumeGroupType { get; set; }

    public long ListSettingDisplayStartDatetime { get; set; }
}
