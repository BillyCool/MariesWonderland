using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBigHuntLink
{
    public int BigHuntLinkId { get; set; }

    public DomainType DestinationDomainType { get; set; }

    public int DestinationDomainId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }
}
