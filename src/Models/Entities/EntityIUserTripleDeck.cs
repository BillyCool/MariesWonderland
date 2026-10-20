using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(DeckType), nameof(UserDeckNumber))]
public class EntityIUserTripleDeck : IUserEntity
{
    public long UserId { get; set; }

    public DeckType DeckType { get; set; }

    public int UserDeckNumber { get; set; }

    public string Name { get; set; }

    public int DeckNumber01 { get; set; }

    public int DeckNumber02 { get; set; }

    public int DeckNumber03 { get; set; }

    public long LatestVersion { get; set; }
}
