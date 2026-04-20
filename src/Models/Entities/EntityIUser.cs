using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUser : IUserEntity
{
    public long UserId { get; set; }

    public long PlayerId { get; set; }

    public int OsType { get; set; }

    public PlatformType PlatformType { get; set; }

    public int UserRestrictionType { get; set; }

    public long RegisterDatetime { get; set; }

    public long GameStartDatetime { get; set; }

    public int BirthYear { get; set; }

    public int BirthMonth { get; set; }

    public long LatestVersion { get; set; }
}
