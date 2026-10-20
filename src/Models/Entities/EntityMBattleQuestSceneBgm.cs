using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleQuestSceneBgm))]
[PrimaryKey(nameof(QuestSceneId), nameof(StartWaveNumber))]
public class EntityMBattleQuestSceneBgm
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int StartWaveNumber { get; set; }

    [Key(2)] public int BgmId { get; set; }

    [Key(3)] public int Stem { get; set; }

    [Key(4)] public int TrackNumber { get; set; }
}
