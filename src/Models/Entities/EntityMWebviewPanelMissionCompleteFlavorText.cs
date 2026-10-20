using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWebviewPanelMissionCompleteFlavorText))]
[PrimaryKey(nameof(WebviewPanelMissionCompleteFlavorTextId), nameof(LanguageType))]
public class EntityMWebviewPanelMissionCompleteFlavorText
{
    [Key(0)] public int WebviewPanelMissionCompleteFlavorTextId { get; set; }

    [Key(1)] public LanguageType LanguageType { get; set; }

    [Key(2)] public string CompleteFlavorText { get; set; }
}
