using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPartsPresetTag
{
    public long UserId { get; set; }

    public int UserPartsPresetTagNumber { get; set; }

    public string Name { get; set; }

    public long LatestVersion { get; set; }
}
