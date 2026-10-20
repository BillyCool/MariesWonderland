using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestSceneChoice))]
[PrimaryKey(nameof(MainFlowQuestSceneId), nameof(QuestFlowType), nameof(ChoiceNumber))]
public class EntityMQuestSceneChoice
{
    [Key(0)] public int MainFlowQuestSceneId { get; set; }

    [Key(1)] public QuestFlowType QuestFlowType { get; set; }

    [Key(2)] public int ChoiceNumber { get; set; }

    [Key(3)] public int QuestSceneChoiceEffectId { get; set; }
}
