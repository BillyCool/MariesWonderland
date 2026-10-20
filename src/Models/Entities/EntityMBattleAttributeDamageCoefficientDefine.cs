using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleAttributeDamageCoefficientDefine))]
[PrimaryKey(nameof(BattleSchemeType))]
public class EntityMBattleAttributeDamageCoefficientDefine
{
    [Key(0)] public BattleSchemeType BattleSchemeType { get; set; }

    [Key(1)] public int PlayerAttributeDamageCoefficientGroupId { get; set; }

    [Key(2)] public int NpcAttributeDamageCoefficientGroupId { get; set; }
}
