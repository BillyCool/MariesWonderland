using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserThoughtUuid))]
public class EntityIUserThought : IUserEntity
{
    public long UserId { get; set; }

    public string UserThoughtUuid { get; set; }

    public int ThoughtId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
