using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckCharacterDropCategory
{
    public long BattleNpcId { get; set; }

    public string BattleNpcDeckCharacterUuid { get; set; }

    public int BattleDropCategoryId { get; set; }
}
