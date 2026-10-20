using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterBoardAbility))]
[PrimaryKey(nameof(BattleNpcId), nameof(CharacterId), nameof(AbilityId))]
public class EntityMBattleNpcCharacterBoardAbility
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int Level { get; set; }
}
