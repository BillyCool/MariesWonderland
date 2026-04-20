namespace MariesWonderland.Models.Entities;

public class EntitySUser : IUserEntity
{
    public long UserId { get; set; }

    public string BackupToken { get; set; } = "";

    public long ChargeMoneyThisMonth { get; set; }
}
