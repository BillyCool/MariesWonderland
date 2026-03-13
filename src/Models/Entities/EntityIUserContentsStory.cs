using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserContentsStory
{
    public long UserId { get; set; }

    public int ContentsStoryId { get; set; }

    public long PlayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
