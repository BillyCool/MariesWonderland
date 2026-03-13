using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMOmikuji
{
    public int OmikujiId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int OmikujiAssetId { get; set; }
}
