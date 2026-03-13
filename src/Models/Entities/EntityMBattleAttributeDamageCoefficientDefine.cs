using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleAttributeDamageCoefficientDefine
{
    public BattleSchemeType BattleSchemeType { get; set; }

    public int PlayerAttributeDamageCoefficientGroupId { get; set; }

    public int NpcAttributeDamageCoefficientGroupId { get; set; }
}
