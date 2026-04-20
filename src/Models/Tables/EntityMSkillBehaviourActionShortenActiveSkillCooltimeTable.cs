using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionShortenActiveSkillCooltimeTable : TableBase<EntityMSkillBehaviourActionShortenActiveSkillCooltime>
{
    private readonly Func<EntityMSkillBehaviourActionShortenActiveSkillCooltime, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionShortenActiveSkillCooltimeTable(EntityMSkillBehaviourActionShortenActiveSkillCooltime[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionShortenActiveSkillCooltime FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
