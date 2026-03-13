using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTipGroupBackgroundAssetRelation
{
    public int TipGroupId { get; set; }

    public int TipBackgroundAssetId { get; set; }

    public int TipDisplayConditionGroupId { get; set; }
}
