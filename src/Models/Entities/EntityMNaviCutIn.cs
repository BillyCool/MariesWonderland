using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMNaviCutIn
{
    public int NaviCutInId { get; set; }

    public CutInFunctionType RelatedCutInFunctionType { get; set; }

    public int SortOrder { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int NaviCutInContentGroupId { get; set; }

    public int RelatedCutInFunctionValue { get; set; }
}
