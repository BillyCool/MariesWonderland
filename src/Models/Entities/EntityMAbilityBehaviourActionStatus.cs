using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviourActionStatus))]
[PrimaryKey(nameof(AbilityBehaviourActionId))]
public class EntityMAbilityBehaviourActionStatus
{
    [Key(0)] public int AbilityBehaviourActionId { get; set; }

    [Key(1)] public AbilityBehaviourStatusChangeType AbilityBehaviourStatusChangeType { get; set; }

    [Key(2)] public AttributeConditionType AttributeConditionType { get; set; }

    [Key(3)] public AbilityBehaviourStatusOrganizationConditionType AbilityOrganizationConditionType { get; set; }

    [Key(4)] public int AbilityStatusId { get; set; }

    [Key(5)] public AbilityBehaviourStatusApplyScopeType ApplyScopeType { get; set; }
}
