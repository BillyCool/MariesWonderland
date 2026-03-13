using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMomBanner
{
    public int MomBannerId { get; set; }

    public int SortOrderDesc { get; set; }

    public DomainType DestinationDomainType { get; set; }

    public int DestinationDomainId { get; set; }

    public string BannerAssetName { get; set; }

    public bool IsEmphasis { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public TargetUserStatusType TargetUserStatusType { get; set; }
}
