namespace MariesWonderland.Models.Entities;

public class EntitySUserGift : IUserEntity
{
    public long UserId { get; set; }

    public string UserGiftUuid { get; set; } = string.Empty;

    public int PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }

    public long GrantDatetime { get; set; }

    public int DescriptionGiftTextId { get; set; }

    public byte[] EquipmentData { get; set; } = [];

    public long ExpirationDatetime { get; set; }

    /// <summary>Unix milliseconds. Zero means the gift has not been received yet.</summary>
    public long ReceivedDatetime { get; set; }
}
