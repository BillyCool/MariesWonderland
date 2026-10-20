using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleQuestSceneBgmSetGroup))]
[PrimaryKey(nameof(QuestSceneId))]
public class EntityMBattleQuestSceneBgmSetGroup
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int BgmSetGroupId { get; set; }
}
