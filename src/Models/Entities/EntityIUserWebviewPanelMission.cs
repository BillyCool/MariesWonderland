using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserWebviewPanelMission : IUserEntity
{
    public long UserId { get; set; }

    public int WebviewPanelMissionPageId { get; set; }

    public long RewardReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
