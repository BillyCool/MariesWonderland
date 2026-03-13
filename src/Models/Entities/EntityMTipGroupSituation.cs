using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTipGroupSituation
{
    public TipSituationType TipSituationType { get; set; }

    public int TipGroupId { get; set; }

    public int Weight { get; set; }

    public int TargetId { get; set; }
}
