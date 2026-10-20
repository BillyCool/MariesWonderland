using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestDisplayItemGroup))]
[PrimaryKey(nameof(EventQuestDisplayItemGroupId), nameof(SortOrder))]
public class EntityMEventQuestDisplayItemGroup
{
    [Key(0)] public int EventQuestDisplayItemGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public PossessionType PossessionType { get; set; }

    [Key(3)] public int PossessionId { get; set; }
}
