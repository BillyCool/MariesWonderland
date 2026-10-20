using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleCostumeSkillFireAct))]
[PrimaryKey(nameof(CostumeId))]
public class EntityMBattleCostumeSkillFireAct
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int BattleSkillFireActId { get; set; }
}
