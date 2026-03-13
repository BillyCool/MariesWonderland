using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleAttributeDamageCoefficientDefineTable : TableBase<EntityMBattleAttributeDamageCoefficientDefine>
{
    private readonly Func<EntityMBattleAttributeDamageCoefficientDefine, BattleSchemeType> primaryIndexSelector;

    public EntityMBattleAttributeDamageCoefficientDefineTable(EntityMBattleAttributeDamageCoefficientDefine[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleSchemeType;
    }

    public EntityMBattleAttributeDamageCoefficientDefine FindByBattleSchemeType(BattleSchemeType key) => FindUniqueCore(data, primaryIndexSelector, Comparer<BattleSchemeType>.Default, key);
}
