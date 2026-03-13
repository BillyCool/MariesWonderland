using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCharacterBoardAbility
{
    public long UserId { get; set; }

    public int CharacterId { get; set; }

    public int AbilityId { get; set; }

    public int Level { get; set; }

    public long LatestVersion { get; set; }
}
