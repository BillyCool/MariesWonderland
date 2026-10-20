using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGiftText))]
[PrimaryKey(nameof(GiftTextId), nameof(LanguageType))]
public class EntityMGiftText
{
    [Key(0)] public int GiftTextId { get; set; }

    [Key(1)] public LanguageType LanguageType { get; set; }

    [Key(2)] public string Text { get; set; }
}
