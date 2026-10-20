using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickExtraQuest))]
[PrimaryKey(nameof(GimmickId), nameof(GimmickOrnamentIndex))]
public class EntityMGimmickExtraQuest
{
    [Key(0)] public int GimmickId { get; set; }

    [Key(1)] public int GimmickOrnamentIndex { get; set; }

    [Key(2)] public int ExtraQuestId { get; set; }
}
