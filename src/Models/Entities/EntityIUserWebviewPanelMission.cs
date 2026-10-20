using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(WebviewPanelMissionPageId))]
public class EntityIUserWebviewPanelMission : IUserEntity
{
    public long UserId { get; set; }

    public int WebviewPanelMissionPageId { get; set; }

    public long RewardReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
