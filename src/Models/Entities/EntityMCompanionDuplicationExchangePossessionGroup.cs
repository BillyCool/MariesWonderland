using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionDuplicationExchangePossessionGroup))]
[PrimaryKey(nameof(CompanionId), nameof(PossessionType))]
public class EntityMCompanionDuplicationExchangePossessionGroup
{
    [Key(0)] public int CompanionId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int Count { get; set; }
}
