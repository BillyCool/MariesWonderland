using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMAbilityBehaviourActionStatusDown
{
    public int AbilityBehaviourActionId { get; set; }

    public AbilityBehaviourStatusChangeType AbilityBehaviourStatusChangeType { get; set; }

    public AttributeConditionType AttributeConditionType { get; set; }

    public AbilityBehaviourStatusOrganizationConditionType AbilityOrganizationConditionType { get; set; }

    public int AbilityStatusId { get; set; }

    public AbilityBehaviourStatusApplyScopeType ApplyScopeType { get; set; }
}
