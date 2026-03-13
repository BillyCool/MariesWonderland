using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMAbilityStatus
{
    public int AbilityStatusId { get; set; }

    public int Agility { get; set; }

    public int Attack { get; set; }

    public int CriticalAttackRatioPermil { get; set; }

    public int CriticalRatioPermil { get; set; }

    public int EvasionRatioPermil { get; set; }

    public int Hp { get; set; }

    public int Vitality { get; set; }
}
