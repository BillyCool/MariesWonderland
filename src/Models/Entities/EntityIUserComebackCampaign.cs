using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserComebackCampaign : IUserEntity
{
    public long UserId { get; set; }

    public int ComebackCampaignId { get; set; }

    public long ComebackDatetime { get; set; }

    public long LatestVersion { get; set; }
}
