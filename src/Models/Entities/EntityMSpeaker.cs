using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSpeaker
{
    public int ActorObjectId { get; set; }

    public int NameSpeakerTextId { get; set; }

    public SpeakerIconType SpeakerIconType { get; set; }

    public int SpeakerIconIndex { get; set; }
}
