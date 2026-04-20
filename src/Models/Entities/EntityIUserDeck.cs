using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeck : IUserEntity
{
    public long UserId { get; set; }

    public DeckType DeckType { get; set; }

    public int UserDeckNumber { get; set; }

    public string UserDeckCharacterUuid01 { get; set; }

    public string UserDeckCharacterUuid02 { get; set; }

    public string UserDeckCharacterUuid03 { get; set; }

    public string Name { get; set; }

    public int Power { get; set; }

    public long LatestVersion { get; set; }
}
