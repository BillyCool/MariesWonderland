using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMDokan
{
    public int DokanId { get; set; }

    public int SortOrder { get; set; }

    public DokanType DokanType { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int DokanContentGroupId { get; set; }

    public TargetUserStatusType TargetUserStatusType { get; set; }

    public MainFunctionType UnlockMainFunctionType { get; set; }
}
