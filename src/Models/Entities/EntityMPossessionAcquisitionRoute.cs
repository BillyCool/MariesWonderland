using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPossessionAcquisitionRoute))]
public class EntityMPossessionAcquisitionRoute
{
    [Key(0)] public PossessionType PossessionType { get; set; }

    [Key(1)] public int PossessionId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public TransitionRouteType AcquisitionRouteType { get; set; }

    [Key(4)] public int RouteId { get; set; }

    [Key(5)] public string RelationValue { get; set; }

    [Key(6)] public long StartDatetime { get; set; }

    [Key(7)] public long EndDatetime { get; set; }
}
