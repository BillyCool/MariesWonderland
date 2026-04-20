using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleEventGroup))]
public class EntityMBattleEventGroup
{
    [Key(0)] public int BattleEventGroupId { get; set; }

    [Key(1)] public int BattleEventId { get; set; }
}
