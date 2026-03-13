using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMTipGroupSituationSeason
{
    public TipSituationType TipSituationType { get; set; }

    public int TipSituationSeasonId { get; set; }

    public int TipGroupId { get; set; }

    public int Weight { get; set; }

    public int TargetId { get; set; }
}
