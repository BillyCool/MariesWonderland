using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleAdditionalAbility
{
    public int BattleGroupId { get; set; }

    public int TargetActorAppearanceWaveNumber { get; set; }

    public int AbilityIndex { get; set; }

    public int AdditionalAbilityApplyScopeType { get; set; }

    public int AbilityId { get; set; }

    public int AbilityLevel { get; set; }
}
