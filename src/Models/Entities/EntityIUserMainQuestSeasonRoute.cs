using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserMainQuestSeasonRoute : IUserEntity
{
    public long UserId { get; set; }

    public int MainQuestSeasonId { get; set; }

    public int MainQuestRouteId { get; set; }

    public long LatestVersion { get; set; }
}
