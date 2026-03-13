using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserGimmickUnlock
{
    public long UserId { get; set; }

    public int GimmickSequenceScheduleId { get; set; }

    public int GimmickSequenceId { get; set; }

    public int GimmickId { get; set; }

    public bool IsUnlocked { get; set; }

    public long LatestVersion { get; set; }
}
