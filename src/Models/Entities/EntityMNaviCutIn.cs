using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNaviCutIn))]
public class EntityMNaviCutIn
{
    [Key(0)] public int NaviCutInId { get; set; }

    [Key(1)] public CutInFunctionType RelatedCutInFunctionType { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public long StartDatetime { get; set; }

    [Key(4)] public long EndDatetime { get; set; }

    [Key(5)] public int NaviCutInContentGroupId { get; set; }

    [Key(6)] public int RelatedCutInFunctionValue { get; set; }
}
