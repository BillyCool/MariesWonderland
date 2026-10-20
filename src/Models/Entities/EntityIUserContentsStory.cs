using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(ContentsStoryId))]
public class EntityIUserContentsStory : IUserEntity
{
    public long UserId { get; set; }

    public int ContentsStoryId { get; set; }

    public long PlayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
