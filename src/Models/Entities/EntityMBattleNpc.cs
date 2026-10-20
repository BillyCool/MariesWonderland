using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpc))]
[PrimaryKey(nameof(BattleNpcId))]
public class EntityMBattleNpc
{
    [Key(0)] public long BattleNpcId { get; set; }
}
