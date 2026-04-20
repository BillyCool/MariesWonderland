using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDisplaySwitch))]
public class EntityMCostumeDisplaySwitch
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int DisplayConditionClearQuestId { get; set; }

    [Key(2)] public int DisplayDeletedExpressionAssetId { get; set; }
}
