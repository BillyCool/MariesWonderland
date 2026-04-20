using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMListSettingAbilityGroup))]
public class EntityMListSettingAbilityGroup
{
    [Key(0)] public int ListSettingAbilityGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int ListSettingAbilityGroupTargetId { get; set; }

    [Key(3)] public int AssetId { get; set; }

    [Key(4)] public ListSettingAbilityGroupType ListSettingAbilityGroupType { get; set; }

    [Key(5)] public long ListSettingDisplayStartDatetime { get; set; }
}
