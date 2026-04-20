using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCageOrnamentStillReleaseCondition))]
public class EntityMCageOrnamentStillReleaseCondition
{
    [Key(0)] public int CageOrnamentStillReleaseConditionId { get; set; }

    [Key(1)] public int CageOrnamentId { get; set; }
}
