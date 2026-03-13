using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCharacterBoard
{
    public long BattleNpcId { get; set; }

    public int CharacterBoardId { get; set; }

    public int PanelReleaseBit1 { get; set; }

    public int PanelReleaseBit2 { get; set; }

    public int PanelReleaseBit3 { get; set; }

    public int PanelReleaseBit4 { get; set; }
}
