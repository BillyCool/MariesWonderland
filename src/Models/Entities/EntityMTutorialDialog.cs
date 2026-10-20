using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTutorialDialog))]
[PrimaryKey(nameof(TutorialType))]
public class EntityMTutorialDialog
{
    [Key(0)] public TutorialType TutorialType { get; set; }

    [Key(1)] public HelpType HelpType { get; set; }
}
