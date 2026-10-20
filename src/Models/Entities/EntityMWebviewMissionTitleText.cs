using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWebviewMissionTitleText))]
[PrimaryKey(nameof(WebviewMissionTitleTextId), nameof(LanguageType))]
public class EntityMWebviewMissionTitleText
{
    [Key(0)] public int WebviewMissionTitleTextId { get; set; }

    [Key(1)] public LanguageType LanguageType { get; set; }

    [Key(2)] public string Text { get; set; }
}
