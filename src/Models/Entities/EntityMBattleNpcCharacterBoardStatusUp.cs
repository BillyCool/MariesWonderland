using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterBoardStatusUp))]
public class EntityMBattleNpcCharacterBoardStatusUp
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public StatusCalculationType StatusCalculationType { get; set; }

    [Key(3)] public int Hp { get; set; }

    [Key(4)] public int Attack { get; set; }

    [Key(5)] public int Vitality { get; set; }

    [Key(6)] public int Agility { get; set; }

    [Key(7)] public int CriticalRatio { get; set; }

    [Key(8)] public int CriticalAttack { get; set; }
}
