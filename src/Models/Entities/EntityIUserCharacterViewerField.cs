using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CharacterViewerFieldId))]
public class EntityIUserCharacterViewerField : IUserEntity
{
    public long UserId { get; set; }

    public int CharacterViewerFieldId { get; set; }

    public long ReleaseDatetime { get; set; }

    public long LatestVersion { get; set; }
}
