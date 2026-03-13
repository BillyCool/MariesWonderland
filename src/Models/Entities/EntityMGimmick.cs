using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMGimmick
{
    public int GimmickId { get; set; }

    public GimmickType GimmickType { get; set; }

    public int GimmickOrnamentGroupId { get; set; }

    public int ClearEvaluateConditionId { get; set; }

    public int ReleaseEvaluateConditionId { get; set; }
}
