using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNaviCutInText))]
public class EntityMNaviCutInText
{
    [Key(0)] public int NaviCutInTextId { get; set; }

    [Key(1)] public LanguageType LanguageType { get; set; }

    [Key(2)] public string Text { get; set; }
}
