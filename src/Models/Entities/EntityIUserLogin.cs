using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserLogin
{
    public long UserId { get; set; }

    public int TotalLoginCount { get; set; }

    public int ContinualLoginCount { get; set; }

    public int MaxContinualLoginCount { get; set; }

    public long LastLoginDatetime { get; set; }

    public long LastComebackLoginDatetime { get; set; }

    public long LatestVersion { get; set; }
}
