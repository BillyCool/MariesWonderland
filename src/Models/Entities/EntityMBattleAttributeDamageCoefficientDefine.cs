using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleAttributeDamageCoefficientDefine))]
public class EntityMBattleAttributeDamageCoefficientDefine
{
    [Key(0)] public BattleSchemeType BattleSchemeType { get; set; }

    [Key(1)] public int PlayerAttributeDamageCoefficientGroupId { get; set; }

    [Key(2)] public int NpcAttributeDamageCoefficientGroupId { get; set; }
}
