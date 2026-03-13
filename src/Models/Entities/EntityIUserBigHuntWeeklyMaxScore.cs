using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserBigHuntWeeklyMaxScore
{
    public long UserId { get; set; }

    public long BigHuntWeeklyVersion { get; set; }

    public AttributeType AttributeType { get; set; }

    public long MaxScore { get; set; }

    public long LatestVersion { get; set; }
}
