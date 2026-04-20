using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleCostumeSkillSe))]
public class EntityMBattleCostumeSkillSe
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int CostumeSkillReadySeAssetId { get; set; }
}
