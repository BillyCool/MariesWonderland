using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionLink))]
[PrimaryKey(nameof(MissionLinkId))]
public class EntityMMissionLink
{
    [Key(0)] public int MissionLinkId { get; set; }

    [Key(1)] public DomainType DestinationDomainType { get; set; }

    [Key(2)] public int DestinationDomainId { get; set; }
}
