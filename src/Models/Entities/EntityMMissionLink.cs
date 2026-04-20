using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionLink))]
public class EntityMMissionLink
{
    [Key(0)] public int MissionLinkId { get; set; }

    [Key(1)] public DomainType DestinationDomainType { get; set; }

    [Key(2)] public int DestinationDomainId { get; set; }
}
