using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestTowerAsset))]
[PrimaryKey(nameof(EventQuestChapterId))]
public class EntityMEventQuestTowerAsset
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int AssetId { get; set; }
}
