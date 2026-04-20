using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCostume : IUserEntity
{
    public long UserId { get; set; }

    public string UserCostumeUuid { get; set; }

    public int CostumeId { get; set; }

    public int LimitBreakCount { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }

    public int HeadupDisplayViewId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public int AwakenCount { get; set; }

    public long LatestVersion { get; set; }
}
