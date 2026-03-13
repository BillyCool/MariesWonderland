using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMAbilityBehaviour
{
    public int AbilityBehaviourId { get; set; }

    public AbilityBehaviourType AbilityBehaviourType { get; set; }

    public int AbilityBehaviourActionId { get; set; }
}
