using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLimitedOpenText
{
    public LimitedOpenTargetType LimitedOpenTargetType { get; set; }

    public int TargetId { get; set; }

    public int OpenAchievementTextAssetId { get; set; }

    public int LocalPushTextAssetId { get; set; }

    public int OpenAchievementTextGroupId { get; set; }
}
