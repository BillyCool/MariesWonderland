using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeaponAbilityReevaluate))]
public class EntityMBattleNpcWeaponAbilityReevaluate
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public long LastReevaluateDatetime { get; set; }
}
