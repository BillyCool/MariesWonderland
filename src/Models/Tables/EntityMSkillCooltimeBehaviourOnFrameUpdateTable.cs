using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourOnFrameUpdateTable : TableBase<EntityMSkillCooltimeBehaviourOnFrameUpdate>
{
    private readonly Func<EntityMSkillCooltimeBehaviourOnFrameUpdate, int> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourOnFrameUpdateTable(EntityMSkillCooltimeBehaviourOnFrameUpdate[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCooltimeBehaviourActionId;
    }

    public bool TryFindBySkillCooltimeBehaviourActionId(int key, out EntityMSkillCooltimeBehaviourOnFrameUpdate result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
