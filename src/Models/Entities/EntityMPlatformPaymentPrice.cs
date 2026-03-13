using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPlatformPaymentPrice
{
    public int PlatformPaymentId { get; set; }

    public PlatformType PlatformType { get; set; }

    public int CurrencyType { get; set; }

    public decimal Price { get; set; }
}
