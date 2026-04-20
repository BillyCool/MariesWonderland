using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPartsPreset : IUserEntity
{
    public long UserId { get; set; }

    public int UserPartsPresetNumber { get; set; }

    public string UserPartsUuid01 { get; set; }

    public string UserPartsUuid02 { get; set; }

    public string UserPartsUuid03 { get; set; }

    public string Name { get; set; }

    public int UserPartsPresetTagNumber { get; set; }

    public long LatestVersion { get; set; }
}
