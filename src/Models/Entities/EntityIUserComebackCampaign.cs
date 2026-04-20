using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserComebackCampaign : IUserEntity
{
    public long UserId { get; set; }

    public int ComebackCampaignId { get; set; }

    public long ComebackDatetime { get; set; }

    public long LatestVersion { get; set; }
}
