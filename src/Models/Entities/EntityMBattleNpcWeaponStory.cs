using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcWeaponStory
{
    public long BattleNpcId { get; set; }

    public int WeaponId { get; set; }

    public int ReleasedMaxStoryIndex { get; set; }
}
