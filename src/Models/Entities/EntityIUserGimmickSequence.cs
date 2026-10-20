using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(GimmickSequenceScheduleId))]
public class EntityIUserGimmickSequence : IUserEntity
{
    public long UserId { get; set; }

    public int GimmickSequenceScheduleId { get; set; }

    public int GimmickSequenceId { get; set; }

    public bool IsGimmickSequenceCleared { get; set; }

    public long ClearDatetime { get; set; }

    public long LatestVersion { get; set; }
}
