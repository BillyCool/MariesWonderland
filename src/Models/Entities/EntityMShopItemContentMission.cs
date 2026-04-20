using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopItemContentMission))]
public class EntityMShopItemContentMission
{
    [Key(0)] public int ShopItemId { get; set; }

    [Key(1)] public int MissionGroupId { get; set; }

    [Key(2)] public bool IsReevaluateOnGrant { get; set; }
}
