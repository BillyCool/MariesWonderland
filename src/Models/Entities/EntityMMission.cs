using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMission))]
public class EntityMMission
{
    [Key(0)] public int MissionId { get; set; }

    [Key(1)] public int MissionGroupId { get; set; }

    [Key(2)] public int SortOrderInMissionGroup { get; set; }

    [Key(3)] public int MissionUnlockConditionId { get; set; }

    [Key(4)] public bool IsNotShowBeforeClear { get; set; }

    [Key(5)] public int NameMissionTextId { get; set; }

    [Key(6)] public int MissionLinkId { get; set; }

    [Key(7)] public MissionClearConditionType MissionClearConditionType { get; set; }

    [Key(8)] public int MissionClearConditionGroupId { get; set; }

    [Key(9)] public int ClearConditionValue { get; set; }

    [Key(10)] public int MissionClearConditionOptionGroupId { get; set; }

    [Key(11)] public int MissionRewardId { get; set; }

    [Key(12)] public int MissionTermId { get; set; }

    [Key(13)] public int MinExpirationDays { get; set; }

    [Key(14)] public MainFunctionType RelatedMainFunctionType { get; set; }

    [Key(15)] public int MissionClearConditionOptionDetailGroupId { get; set; }

    [Key(16)] public int MissionUnlockConditionDetailGroupId { get; set; }
}
