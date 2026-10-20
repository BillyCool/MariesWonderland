using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWebviewMission))]
[PrimaryKey(nameof(WebviewMissionId))]
public class EntityMWebviewMission
{
    [Key(0)] public int WebviewMissionId { get; set; }

    [Key(1)] public int TitleTextId { get; set; }

    [Key(2)] public int WebviewMissionType { get; set; }

    [Key(3)] public int WebviewMissionTargetId { get; set; }

    [Key(4)] public long StartDatetime { get; set; }

    [Key(5)] public long EndDatetime { get; set; }
}
