using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(MissionId))]
public class EntityIUserMissionCompletionProgress : IUserEntity
{
    public long UserId { get; set; }

    public int MissionId { get; set; }

    public long ProgressValue { get; set; }

    public long LatestVersion { get; set; }
}
