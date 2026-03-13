using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTip
{
    public int TipId { get; set; }

    public int TitleTipTextId { get; set; }

    public int ContentTipTextId { get; set; }

    public int TipDisplayConditionGroupId { get; set; }

    public string BackgroundAssetName { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
