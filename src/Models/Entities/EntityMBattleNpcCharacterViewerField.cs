using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterViewerField))]
[PrimaryKey(nameof(BattleNpcId), nameof(CharacterViewerFieldId))]
public class EntityMBattleNpcCharacterViewerField
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterViewerFieldId { get; set; }

    [Key(2)] public long ReleaseDatetime { get; set; }
}
