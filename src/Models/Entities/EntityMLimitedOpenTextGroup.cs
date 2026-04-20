using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLimitedOpenTextGroup))]
public class EntityMLimitedOpenTextGroup
{
    [Key(0)] public int LimitedOpenTextGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public LimitedOpenTextDisplayConditionType LimitedOpenTextDisplayConditionType { get; set; }

    [Key(3)] public int LimitedOpenTextDisplayConditionValue { get; set; }

    [Key(4)] public int TextAssetId { get; set; }
}
