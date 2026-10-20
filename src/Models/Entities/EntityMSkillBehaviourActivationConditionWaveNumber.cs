using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActivationConditionWaveNumber))]
[PrimaryKey(nameof(SkillBehaviourActivationConditionId))]
public class EntityMSkillBehaviourActivationConditionWaveNumber
{
    [Key(0)] public int SkillBehaviourActivationConditionId { get; set; }

    [Key(1)] public int WaveNumber { get; set; }
}
