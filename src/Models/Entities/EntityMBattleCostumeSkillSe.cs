using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleCostumeSkillSe))]
[PrimaryKey(nameof(CostumeId))]
public class EntityMBattleCostumeSkillSe
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int CostumeSkillReadySeAssetId { get; set; }
}
