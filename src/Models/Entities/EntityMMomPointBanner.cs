using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMomPointBanner
{
    public int MomPointBannerId { get; set; }

    public int BannerAssetId { get; set; }

    public int DestinationInformationId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
