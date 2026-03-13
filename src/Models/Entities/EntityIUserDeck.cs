using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDeck
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

    public EntityIUserDeck(long UserId, DeckType DeckType, int UserDeckNumber, string UserDeckCharacterUuid01,
        string UserDeckCharacterUuid02, string UserDeckCharacterUuid03, string Name, int Power, long LatestVersion)
    {
        this.UserId = UserId;
        this.DeckType = DeckType;
        this.UserDeckNumber = UserDeckNumber;
        this.UserDeckCharacterUuid01 = UserDeckCharacterUuid01;
        this.UserDeckCharacterUuid02 = UserDeckCharacterUuid02;
        this.UserDeckCharacterUuid03 = UserDeckCharacterUuid03;
        this.Name = Name;
        this.Power = Power;
        this.LatestVersion = LatestVersion;
    }
}
