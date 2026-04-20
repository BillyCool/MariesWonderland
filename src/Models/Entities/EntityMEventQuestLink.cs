using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLink))]
public class EntityMEventQuestLink
{
    [Key(0)] public int EventQuestLinkId { get; set; }

    [Key(1)] public DomainType DestinationDomainType { get; set; }

    [Key(2)] public int DestinationDomainId { get; set; }

    [Key(3)] public PossessionType PossessionType { get; set; }

    [Key(4)] public int PossessionId { get; set; }
}
