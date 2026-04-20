using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacter))]
public class EntityMCharacter
{
    [Key(0)] public int CharacterId { get; set; }

    [Key(1)] public int CharacterLevelBonusAbilityGroupId { get; set; }

    [Key(2)] public int NameCharacterTextId { get; set; }

    [Key(3)] public int CharacterAssetId { get; set; }

    [Key(4)] public int SortOrder { get; set; }

    [Key(5)] public int DefaultCostumeId { get; set; }

    [Key(6)] public int DefaultWeaponId { get; set; }

    [Key(7)] public int EndCostumeId { get; set; }

    [Key(8)] public int EndWeaponId { get; set; }

    [Key(9)] public int MaxLevelNumericalFunctionId { get; set; }

    [Key(10)] public int RequiredExpForLevelUpNumericalParameterMapId { get; set; }

    [Key(11)] public int ListSettingCostumeGroupType { get; set; }

    [Key(12)] public long ListSettingDisplayStartDatetime { get; set; }
}
