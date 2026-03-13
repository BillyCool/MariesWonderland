using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLink
{
    public int EventQuestLinkId { get; set; }

    public DomainType DestinationDomainType { get; set; }

    public int DestinationDomainId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }
}
