using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompleteMissionGroup))]
[PrimaryKey(nameof(MissionId), nameof(PossessionType), nameof(PossessionId))]
public class EntityMCompleteMissionGroup
{
    [Key(0)] public int MissionId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int SortOrder { get; set; }
}
