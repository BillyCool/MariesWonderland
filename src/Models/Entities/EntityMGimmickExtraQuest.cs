using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickExtraQuest))]
public class EntityMGimmickExtraQuest
{
    [Key(0)] public int GimmickId { get; set; }

    [Key(1)] public int GimmickOrnamentIndex { get; set; }

    [Key(2)] public int ExtraQuestId { get; set; }
}
