using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMActorAnimation
{
    public int ActorAnimationId { get; set; }

    public int ActorId { get; set; }

    public int ActorAnimationCategoryId { get; set; }

    public int ActorAnimationType { get; set; }

    public string AssetPath { get; set; }

    public bool IsDefault { get; set; }
}
