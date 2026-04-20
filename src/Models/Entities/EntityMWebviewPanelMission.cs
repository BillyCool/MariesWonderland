using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWebviewPanelMission))]
public class EntityMWebviewPanelMission
{
    [Key(0)] public int WebviewPanelMissionId { get; set; }

    [Key(1)] public int Page { get; set; }

    [Key(2)] public int WebviewPanelMissionPageId { get; set; }

    [Key(3)] public long StartDatetime { get; set; }

    [Key(4)] public long EndDatetime { get; set; }
}
