using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeckPartsGroup
{
    public long UserId { get; set; }

    public string UserDeckCharacterUuid { get; set; }

    public string UserPartsUuid { get; set; }

    public int SortOrder { get; set; }

    public long LatestVersion { get; set; }
}
