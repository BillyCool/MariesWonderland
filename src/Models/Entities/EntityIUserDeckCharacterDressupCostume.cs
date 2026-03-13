using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeckCharacterDressupCostume
{
    public long UserId { get; set; }

    public string UserDeckCharacterUuid { get; set; }

    public int DressupCostumeId { get; set; }

    public long LatestVersion { get; set; }
}
