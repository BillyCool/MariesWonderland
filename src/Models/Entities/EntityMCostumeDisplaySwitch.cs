using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDisplaySwitch))]
[PrimaryKey(nameof(CostumeId))]
public class EntityMCostumeDisplaySwitch
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int DisplayConditionClearQuestId { get; set; }

    [Key(2)] public int DisplayDeletedExpressionAssetId { get; set; }
}
