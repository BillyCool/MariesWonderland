using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(DokanId))]
public class EntityIUserDokan : IUserEntity
{
    public long UserId { get; set; }

    public int DokanId { get; set; }

    public long DisplayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
