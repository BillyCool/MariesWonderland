using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMParts))]
public class EntityMParts
{
    [Key(0)] public int PartsId { get; set; }

    [Key(1)] public RarityType RarityType { get; set; }

    [Key(2)] public int PartsGroupId { get; set; }

    [Key(3)] public int PartsStatusMainLotteryGroupId { get; set; }

    [Key(4)] public int PartsStatusSubLotteryGroupId { get; set; }

    [Key(5)] public int PartsInitialLotteryId { get; set; }
}
