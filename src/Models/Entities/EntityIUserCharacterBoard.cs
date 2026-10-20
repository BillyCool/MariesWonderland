using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterBoardId))]
public class EntityIUserCharacterBoard : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterBoardId { get; set; }

    public int PanelReleaseBit1 { get; set; }

    public int PanelReleaseBit2 { get; set; }

    public int PanelReleaseBit3 { get; set; }

    public int PanelReleaseBit4 { get; set; }

    public long LatestVersion { get; set; }
}
