using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCharacterBoardStatusUp
{
    public long BattleNpcId { get; set; }

    public int CharacterId { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int Hp { get; set; }

    public int Attack { get; set; }

    public int Vitality { get; set; }

    public int Agility { get; set; }

    public int CriticalRatio { get; set; }

    public int CriticalAttack { get; set; }
}
