using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleProgressUiType))]
public class EntityMBattleProgressUiType
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int BattleProgressUiTypeId { get; set; }
}
