using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(ConsumableItemId))]
public class EntityIUserConsumableItem : IUserEntity
{
    public long UserId { get; set; }

    public int ConsumableItemId { get; set; }

    public int Count { get; set; }

    public long FirstAcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
