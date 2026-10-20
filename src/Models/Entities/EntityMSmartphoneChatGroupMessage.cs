using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSmartphoneChatGroupMessage))]
[PrimaryKey(nameof(SmartphoneChatGroupId), nameof(SortOrder))]
public class EntityMSmartphoneChatGroupMessage
{
    [Key(0)] public int SmartphoneChatGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int SmartphoneMessageUnlockSceneId { get; set; }

    [Key(3)] public int SmartphoneMessageUnlockValue { get; set; }

    [Key(4)] public int SenderCharacterId { get; set; }
}
