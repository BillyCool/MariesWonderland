using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMissionPassMissionGroup))]
[PrimaryKey(nameof(MissionPassId), nameof(MissionGroupId))]
public class EntityMMissionPassMissionGroup
{
    [Key(0)] public int MissionPassId { get; set; }

    [Key(1)] public int MissionGroupId { get; set; }
}
