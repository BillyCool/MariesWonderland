using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(DeckType))]
public class EntityIUserDeckTypeNote : IUserEntity
{
    public long UserId { get; set; }

    public DeckType DeckType { get; set; }

    public int MaxDeckPower { get; set; }

    public long LatestVersion { get; set; }
}
