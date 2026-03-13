using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCageOrnament
{
    public int CageOrnamentId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int CageOrnamentRewardId { get; set; }
}
