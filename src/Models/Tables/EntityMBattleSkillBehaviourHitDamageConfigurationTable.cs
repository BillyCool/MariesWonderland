using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleSkillBehaviourHitDamageConfigurationTable : TableBase<EntityMBattleSkillBehaviourHitDamageConfiguration>
{
    private readonly Func<EntityMBattleSkillBehaviourHitDamageConfiguration, (SkillCategoryType, int, int)> primaryIndexSelector;

    public EntityMBattleSkillBehaviourHitDamageConfigurationTable(EntityMBattleSkillBehaviourHitDamageConfiguration[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillCategoryType, element.HitCount, element.HitIndexLowerLimit);
    }
}
