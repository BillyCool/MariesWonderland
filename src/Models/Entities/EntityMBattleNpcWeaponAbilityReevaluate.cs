using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcWeaponAbilityReevaluate))]
[PrimaryKey(nameof(BattleNpcId))]
public class EntityMBattleNpcWeaponAbilityReevaluate
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public long LastReevaluateDatetime { get; set; }
}
