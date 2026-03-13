using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMHeadupDisplayView
{
    public int HeadupDisplayViewId { get; set; }

    public ViewSkillButtonType ViewSkillButtonType { get; set; }

    public HpBarDisplayType HpBarDisplayType { get; set; }

    public ViewNameTextType ViewNameTextType { get; set; }

    public ViewBuffAbnormalType ViewBuffAbnormalType { get; set; }

    public ViewLevelTextType ViewLevelTextType { get; set; }
}
