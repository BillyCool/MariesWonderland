using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCharacter
{
    public long BattleNpcId { get; set; }

    public int CharacterId { get; set; }

    public int Level { get; set; }

    public int Exp { get; set; }
}
