using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCharacterViewerField
{
    public long UserId { get; set; }

    public int CharacterViewerFieldId { get; set; }

    public long ReleaseDatetime { get; set; }

    public long LatestVersion { get; set; }
}
