using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMListSettingAbilityGroup))]
[PrimaryKey(nameof(ListSettingAbilityGroupId))]
public class EntityMListSettingAbilityGroup
{
    [Key(0)] public int ListSettingAbilityGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int ListSettingAbilityGroupTargetId { get; set; }

    [Key(3)] public int AssetId { get; set; }

    [Key(4)] public ListSettingAbilityGroupType ListSettingAbilityGroupType { get; set; }

    [Key(5)] public long ListSettingDisplayStartDatetime { get; set; }
}
