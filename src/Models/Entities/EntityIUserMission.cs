using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMission
{
    public long UserId { get; set; }

    public int MissionId { get; set; }

    public long StartDatetime { get; set; }

    public int ProgressValue { get; set; }

    public MissionProgressStatusType MissionProgressStatusType { get; set; }

    public long ClearDatetime { get; set; }

    public long LatestVersion { get; set; }
}
