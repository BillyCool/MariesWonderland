using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckCharacter))]
public class EntityMBattleNpcDeckCharacter
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcDeckCharacterUuid { get; set; }

    [Key(2)] public string BattleNpcCostumeUuid { get; set; }

    [Key(3)] public string MainBattleNpcWeaponUuid { get; set; }

    [Key(4)] public string BattleNpcCompanionUuid { get; set; }

    [Key(5)] public int Power { get; set; }

    [Key(6)] public string BattleNpcThoughtUuid { get; set; }
}
