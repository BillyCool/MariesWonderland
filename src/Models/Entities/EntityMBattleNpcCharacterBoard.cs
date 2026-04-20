using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterBoard))]
public class EntityMBattleNpcCharacterBoard
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterBoardId { get; set; }

    [Key(2)] public int PanelReleaseBit1 { get; set; }

    [Key(3)] public int PanelReleaseBit2 { get; set; }

    [Key(4)] public int PanelReleaseBit3 { get; set; }

    [Key(5)] public int PanelReleaseBit4 { get; set; }
}
