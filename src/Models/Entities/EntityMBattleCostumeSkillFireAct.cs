using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleCostumeSkillFireAct))]
public class EntityMBattleCostumeSkillFireAct
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int BattleSkillFireActId { get; set; }
}
