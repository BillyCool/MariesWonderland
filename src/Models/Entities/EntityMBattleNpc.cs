using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpc))]
public class EntityMBattleNpc
{
    [Key(0)] public long BattleNpcId { get; set; }
}
