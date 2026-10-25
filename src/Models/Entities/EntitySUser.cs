using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntitySUser : IUserEntity
{
    public long UserId { get; set; }

    public string BackupToken { get; set; } = "";

    public long ChargeMoneyThisMonth { get; set; }

    /// <summary>Set by <c>SetBirthYearMonth</c>; 0 until then. The client never has this in its user tables.</summary>
    public int BirthYear { get; set; }

    public int BirthMonth { get; set; }
}
