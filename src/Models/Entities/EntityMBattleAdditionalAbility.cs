using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleAdditionalAbility))]
public class EntityMBattleAdditionalAbility
{
    [Key(0)] public int BattleGroupId { get; set; }

    [Key(1)] public int TargetActorAppearanceWaveNumber { get; set; }

    [Key(2)] public int AbilityIndex { get; set; }

    [Key(3)] public int AdditionalAbilityApplyScopeType { get; set; }

    [Key(4)] public int AbilityId { get; set; }

    [Key(5)] public int AbilityLevel { get; set; }
}
