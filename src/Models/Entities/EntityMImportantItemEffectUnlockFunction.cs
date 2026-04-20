using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItemEffectUnlockFunction))]
public class EntityMImportantItemEffectUnlockFunction
{
    [Key(0)] public int ImportantItemEffectUnlockFunctionId { get; set; }

    [Key(1)] public int ImportantItemEffectUnlockFunctionType { get; set; }

    [Key(2)] public int UnlockFunctionEffectValue { get; set; }
}
