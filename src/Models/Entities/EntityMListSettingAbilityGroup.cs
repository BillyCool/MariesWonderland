using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMListSettingAbilityGroup
{
    public int ListSettingAbilityGroupId { get; set; }

    public int SortOrder { get; set; }

    public int ListSettingAbilityGroupTargetId { get; set; }

    public int AssetId { get; set; }

    public ListSettingAbilityGroupType ListSettingAbilityGroupType { get; set; }

    public long ListSettingDisplayStartDatetime { get; set; }
}
