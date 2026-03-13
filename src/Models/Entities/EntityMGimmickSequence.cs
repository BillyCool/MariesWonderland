using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMGimmickSequence
{
    public int GimmickSequenceId { get; set; }

    public int GimmickSequenceClearConditionType { get; set; }

    public int NextGimmickSequenceGroupId { get; set; }

    public int GimmickGroupId { get; set; }

    public int GimmickSequenceRewardGroupId { get; set; }

    public FlowType FlowType { get; set; }

    public int ProgressRequireHour { get; set; }

    public long ProgressStartDatetime { get; set; }
}
