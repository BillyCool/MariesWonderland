using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(MissionId))]
public class EntityIUserMission : IUserEntity
{
    public long UserId { get; set; }

    public int MissionId { get; set; }

    public long StartDatetime { get; set; }

    public int ProgressValue { get; set; }

    public MissionProgressStatusType MissionProgressStatusType { get; set; }

    public long ClearDatetime { get; set; }

    public long LatestVersion { get; set; }
}
