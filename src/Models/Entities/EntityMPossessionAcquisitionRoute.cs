using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPossessionAcquisitionRoute
{
    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int SortOrder { get; set; }

    public TransitionRouteType AcquisitionRouteType { get; set; }

    public int RouteId { get; set; }

    public string RelationValue { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
