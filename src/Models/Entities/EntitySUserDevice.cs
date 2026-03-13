namespace MariesWonderland.Models.Entities;

public class EntitySUserDevice
{
    public long UserId { get; set; }
    public string Uuid { get; set; } = "";
    public string AdvertisingId { get; set; } = "";
    public bool IsTrackingEnabled { get; set; }
    public string IdentifierForVendor { get; set; } = "";
    public string DeviceToken { get; set; } = "";
    public string MacAddress { get; set; } = "";
    public string RegistrationId { get; set; } = "";
    public long RegisteredAt { get; set; }
    public long LastAuthAt { get; set; }
}
