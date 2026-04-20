using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMUserQuestSceneGrantPossession))]
public class EntityMUserQuestSceneGrantPossession
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int Count { get; set; }

    [Key(4)] public bool IsGift { get; set; }

    [Key(5)] public bool IsDebug { get; set; }
}
