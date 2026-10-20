using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPlatformPayment))]
[PrimaryKey(nameof(PlatformPaymentId), nameof(PlatformType))]
public class EntityMPlatformPayment
{
    [Key(0)] public int PlatformPaymentId { get; set; }

    [Key(1)] public PlatformType PlatformType { get; set; }

    [Key(2)] public string ProductIdSuffix { get; set; }
}
