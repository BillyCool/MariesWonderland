using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPlatformPayment
{
    public int PlatformPaymentId { get; set; }

    public PlatformType PlatformType { get; set; }

    public string ProductIdSuffix { get; set; }
}
