using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTitleStill
{
    public int TitleStillId { get; set; }

    public int TitleStillGroupId { get; set; }

    public int ReleaseEvaluateConditionId { get; set; }

    public TitleStillLogoType TitleStillLogoType { get; set; }

    public string AssetName { get; set; }
}
