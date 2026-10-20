using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCooltimeBehaviourOnFrameUpdate))]
[PrimaryKey(nameof(SkillCooltimeBehaviourActionId))]
public class EntityMSkillCooltimeBehaviourOnFrameUpdate
{
    [Key(0)] public int SkillCooltimeBehaviourActionId { get; set; }

    [Key(1)] public int SkillCooltimeAdvanceValuePerFrame { get; set; }
}
