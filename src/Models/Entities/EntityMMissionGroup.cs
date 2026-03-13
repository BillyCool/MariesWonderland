using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMissionGroup
{
    public int MissionGroupId { get; set; }

    public MissionCategoryType MissionCategoryType { get; set; }

    public int LabelMissionTextId { get; set; }

    public int SortOrderInLabel { get; set; }

    public int AssetId { get; set; }

    public int MissionGroupUnlockConditionGroupId { get; set; }

    public int MissionSubCategoryId { get; set; }
}
