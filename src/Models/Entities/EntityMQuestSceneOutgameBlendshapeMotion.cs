using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestSceneOutgameBlendshapeMotion))]
public class EntityMQuestSceneOutgameBlendshapeMotion
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int ActorAnimationId { get; set; }
}
