using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPortalCageCharacterGroup
{
    public int PortalCageCharacterGroupId { get; set; }

    public int PlayerCharacterActorObjectId { get; set; }

    public int NaviCharacterActorObjectId { get; set; }

    public int NaviMenuActorObjectId { get; set; }

    public TutorialType TutorialType { get; set; }
}
