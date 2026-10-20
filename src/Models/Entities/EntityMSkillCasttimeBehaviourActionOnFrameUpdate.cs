using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSkillCasttimeBehaviourActionOnFrameUpdate))]
[PrimaryKey(nameof(SkillCasttimeBehaviourActionId))]
public class EntityMSkillCasttimeBehaviourActionOnFrameUpdate
{
    [Key(0)] public int SkillCasttimeBehaviourActionId { get; set; }

    [Key(1)] public int SkillCasttimeAdvanceValuePerFrame { get; set; }
}
