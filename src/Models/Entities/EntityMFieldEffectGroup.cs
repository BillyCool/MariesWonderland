using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMFieldEffectGroup))]
public class EntityMFieldEffectGroup
{
    [Key(0)] public int FieldEffectGroupId { get; set; }

    [Key(1)] public int FieldEffectGroupIndex { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int DefaultAbilityLevel { get; set; }

    [Key(4)] public FieldEffectApplyScopeType FieldEffectApplyScopeType { get; set; }

    [Key(5)] public int FieldEffectAssetId { get; set; }
}
