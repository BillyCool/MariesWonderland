using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMDeckEntrustCoefficientStatus))]
public class EntityMDeckEntrustCoefficientStatus
{
    [Key(0)] public int EntrustDeckStatusType { get; set; }

    [Key(1)] public int DeckStatusType { get; set; }

    [Key(2)] public int CoefficientPermil { get; set; }
}
