using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourOnSkillDamageTable : TableBase<EntityMSkillCooltimeBehaviourOnSkillDamage>
{
    private readonly Func<EntityMSkillCooltimeBehaviourOnSkillDamage, int> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourOnSkillDamageTable(EntityMSkillCooltimeBehaviourOnSkillDamage[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCooltimeBehaviourActionId;
    }

    public bool TryFindBySkillCooltimeBehaviourActionId(int key, out EntityMSkillCooltimeBehaviourOnSkillDamage result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
