using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPowerReferenceStatusGroup
{
    public int PowerReferenceStatusGroupId { get; set; }

    public StatusKindType ReferenceStatusType { get; set; }

    public AttributeConditionType AttributeConditionType { get; set; }

    public int CoefficientValuePermil { get; set; }
}
