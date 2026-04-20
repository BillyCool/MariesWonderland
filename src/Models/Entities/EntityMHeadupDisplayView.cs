using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHeadupDisplayView))]
public class EntityMHeadupDisplayView
{
    [Key(0)] public int HeadupDisplayViewId { get; set; }

    [Key(1)] public ViewSkillButtonType ViewSkillButtonType { get; set; }

    [Key(2)] public HpBarDisplayType HpBarDisplayType { get; set; }

    [Key(3)] public ViewNameTextType ViewNameTextType { get; set; }

    [Key(4)] public ViewBuffAbnormalType ViewBuffAbnormalType { get; set; }

    [Key(5)] public ViewLevelTextType ViewLevelTextType { get; set; }
}
