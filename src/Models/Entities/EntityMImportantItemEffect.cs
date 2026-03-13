using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMImportantItemEffect
{
    public int ImportantItemEffectId { get; set; }

    public int ImportantItemEffectGroupingId { get; set; }

    public int Priority { get; set; }

    public int ImportantItemEffectType { get; set; }

    public int ImportantItemEffectTargetId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
