using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterRebirth))]
public class EntityMBattleNpcCharacterRebirth
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public int RebirthCount { get; set; }
}
