using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNaviCutInText))]
[PrimaryKey(nameof(NaviCutInTextId), nameof(LanguageType))]
public class EntityMNaviCutInText
{
    [Key(0)] public int NaviCutInTextId { get; set; }

    [Key(1)] public LanguageType LanguageType { get; set; }

    [Key(2)] public string Text { get; set; }
}
