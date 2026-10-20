using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(GimmickSequenceScheduleId), nameof(GimmickSequenceId), nameof(GimmickId))]
public class EntityIUserGimmickUnlock : IUserEntity
{
    public long UserId { get; set; }

    public int GimmickSequenceScheduleId { get; set; }

    public int GimmickSequenceId { get; set; }

    public int GimmickId { get; set; }

    public bool IsUnlocked { get; set; }

    public long LatestVersion { get; set; }
}
