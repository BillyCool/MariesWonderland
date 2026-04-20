using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterRebirthStepGroup))]
public class EntityMCharacterRebirthStepGroup
{
    [Key(0)] public int CharacterRebirthStepGroupId { get; set; }

    [Key(1)] public int BeforeRebirthCount { get; set; }

    [Key(2)] public int CostumeLevelLimitUp { get; set; }

    [Key(3)] public int CharacterRebirthMaterialGroupId { get; set; }
}
