using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(MainQuestSeasonId))]
public class EntityIUserMainQuestSeasonRoute : IUserEntity
{
    public long UserId { get; set; }

    public int MainQuestSeasonId { get; set; }

    public int MainQuestRouteId { get; set; }

    public long LatestVersion { get; set; }
}
