using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleActorAi))]
[PrimaryKey(nameof(BattleActorAiId))]
public class EntityMBattleActorAi
{
    [Key(0)] public int BattleActorAiId { get; set; }

    [Key(1)] public string AssetPath { get; set; }

    [Key(2)] public string Description { get; set; }
}
