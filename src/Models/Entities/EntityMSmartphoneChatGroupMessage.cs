using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSmartphoneChatGroupMessage
{
    public int SmartphoneChatGroupId { get; set; }

    public int SortOrder { get; set; }

    public int SmartphoneMessageUnlockSceneId { get; set; }

    public int SmartphoneMessageUnlockValue { get; set; }

    public int SenderCharacterId { get; set; }
}
