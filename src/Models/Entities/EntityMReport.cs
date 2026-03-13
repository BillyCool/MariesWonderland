using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMReport
{
    public int ReportId { get; set; }

    public int MainQuestSeasonId { get; set; }

    public int CharacterId { get; set; }

    public int ReportNumber { get; set; }

    public int ReportAssetId { get; set; }
}
