using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLoginBonus
{
    public int LoginBonusId { get; set; }

    public int SortOrder { get; set; }

    public int LoginBonusStartConditionId { get; set; }

    public int TotalPageCount { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public long StampReceiveEndDatetime { get; set; }

    public string LoginBonusAssetName { get; set; }
}
