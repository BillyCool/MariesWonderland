using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTutorialDialog))]
public class EntityMTutorialDialog
{
    [Key(0)] public TutorialType TutorialType { get; set; }

    [Key(1)] public HelpType HelpType { get; set; }
}
