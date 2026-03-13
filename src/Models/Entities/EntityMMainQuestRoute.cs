using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMainQuestRoute
{
    public int MainQuestRouteId { get; set; }

    public int MainQuestSeasonId { get; set; }

    public int SortOrder { get; set; }

    public int CharacterId { get; set; }
}
