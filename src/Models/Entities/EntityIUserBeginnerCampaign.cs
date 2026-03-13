using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserBeginnerCampaign
{
    public long UserId { get; set; }

    public int BeginnerCampaignId { get; set; }

    public long CampaignRegisterDatetime { get; set; }

    public long LatestVersion { get; set; }
}
