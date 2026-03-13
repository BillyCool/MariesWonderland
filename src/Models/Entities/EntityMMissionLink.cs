using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMissionLink
{
    public int MissionLinkId { get; set; }

    public DomainType DestinationDomainType { get; set; }

    public int DestinationDomainId { get; set; }
}
