using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWebviewMission
{
    public int WebviewMissionId { get; set; }

    public int TitleTextId { get; set; }

    public int WebviewMissionType { get; set; }

    public int WebviewMissionTargetId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
