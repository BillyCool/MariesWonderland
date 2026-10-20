using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(GimmickSequenceScheduleId), nameof(GimmickSequenceId), nameof(GimmickId), nameof(GimmickOrnamentIndex))]
public class EntityIUserGimmickOrnamentProgress : IUserEntity
{
    public long UserId { get; set; }

    public int GimmickSequenceScheduleId { get; set; }

    public int GimmickSequenceId { get; set; }

    public int GimmickId { get; set; }

    public int GimmickOrnamentIndex { get; set; }

    public int ProgressValueBit { get; set; }

    public long BaseDatetime { get; set; }

    public long LatestVersion { get; set; }
}
