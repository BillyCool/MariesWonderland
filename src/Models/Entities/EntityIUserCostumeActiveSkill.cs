using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCostumeActiveSkill
{
    public long UserId { get; set; }

    public string UserCostumeUuid { get; set; }

    public int Level { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
