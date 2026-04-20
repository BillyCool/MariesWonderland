using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTipGroupSituationSeason))]
public class EntityMTipGroupSituationSeason
{
    [Key(0)] public TipSituationType TipSituationType { get; set; }

    [Key(1)] public int TipSituationSeasonId { get; set; }

    [Key(2)] public int TipGroupId { get; set; }

    [Key(3)] public int Weight { get; set; }

    [Key(4)] public int TargetId { get; set; }
}
