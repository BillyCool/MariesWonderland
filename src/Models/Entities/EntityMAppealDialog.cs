using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMAppealDialog
{
    public int AppealDialogId { get; set; }

    public int SortOrder { get; set; }

    public AppealTargetType AppealTargetType { get; set; }

    public int AppealTargetId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int TitleTextId { get; set; }

    public int AssetId { get; set; }
}
