using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMActorAnimationController
{
    public int ActorAnimationControllerId { get; set; }

    public int ActorId { get; set; }

    public int ActorAnimationControllerType { get; set; }

    public string AssetPath { get; set; }
}
