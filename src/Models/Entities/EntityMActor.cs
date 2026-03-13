using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMActor
{
    public int ActorId { get; set; }

    public int NameActorTextId { get; set; }

    public string ActorAssetId { get; set; }

    public string ActorSpeakerIconAssetPath { get; set; }

    public string AnimatorAssetPath { get; set; }
}
