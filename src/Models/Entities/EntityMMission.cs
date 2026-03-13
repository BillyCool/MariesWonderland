using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMission
{
    public int MissionId { get; set; }

    public int MissionGroupId { get; set; }

    public int SortOrderInMissionGroup { get; set; }

    public int MissionUnlockConditionId { get; set; }

    public bool IsNotShowBeforeClear { get; set; }

    public int NameMissionTextId { get; set; }

    public int MissionLinkId { get; set; }

    public MissionClearConditionType MissionClearConditionType { get; set; }

    public int MissionClearConditionGroupId { get; set; }

    public int ClearConditionValue { get; set; }

    public int MissionClearConditionOptionGroupId { get; set; }

    public int MissionRewardId { get; set; }

    public int MissionTermId { get; set; }

    public int MinExpirationDays { get; set; }

    public MainFunctionType RelatedMainFunctionType { get; set; }

    public int MissionClearConditionOptionDetailGroupId { get; set; }

    public int MissionUnlockConditionDetailGroupId { get; set; }
}
