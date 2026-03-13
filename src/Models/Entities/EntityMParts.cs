using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMParts
{
    public int PartsId { get; set; }

    public RarityType RarityType { get; set; }

    public int PartsGroupId { get; set; }

    public int PartsStatusMainLotteryGroupId { get; set; }

    public int PartsStatusSubLotteryGroupId { get; set; }

    public int PartsInitialLotteryId { get; set; }
}
