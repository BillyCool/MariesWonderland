using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAwakenItemAcquire))]
[PrimaryKey(nameof(CostumeAwakenItemAcquireId), nameof(PossessionType), nameof(PossessionId))]
public class EntityMCostumeAwakenItemAcquire
{
    [Key(0)] public int CostumeAwakenItemAcquireId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int Count { get; set; }
}
