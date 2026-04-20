using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillBehaviourActionRemoveBuff))]
public class EntityMSkillBehaviourActionRemoveBuff
{
    [Key(0)] public int SkillBehaviourActionId { get; set; }

    [Key(1)] public int RemoveCount { get; set; }

    [Key(2)] public BuffType BuffType { get; set; }

    [Key(3)] public SkillRemoveBuffFilteringType SkillRemoveBuffFilteringType { get; set; }

    [Key(4)] public int SkillRemoveBuffFilteringId { get; set; }

    [Key(5)] public SkillRemoveBuffChoosingType SkillRemoveBuffChoosingType { get; set; }
}
