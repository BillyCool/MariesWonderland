using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMShopReplaceableGem))]
public class EntityMShopReplaceableGem
{
    [Key(0)] public int LineupUpdateCountLowerLimit { get; set; }

    [Key(1)] public int NecessaryGem { get; set; }
}
