using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckCharacterDropCategory))]
public class EntityMBattleNpcDeckCharacterDropCategory
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcDeckCharacterUuid { get; set; }

    [Key(2)] public int BattleDropCategoryId { get; set; }
}
