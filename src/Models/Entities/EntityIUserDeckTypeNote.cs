using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeckTypeNote
{
    public long UserId { get; set; }

    public DeckType DeckType { get; set; }

    public int MaxDeckPower { get; set; }

    public long LatestVersion { get; set; }
}
