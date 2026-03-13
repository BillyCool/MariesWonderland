using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillTable : TableBase<EntityMSkill>
{
    private readonly Func<EntityMSkill, int> primaryIndexSelector;

    public EntityMSkillTable(EntityMSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillId;
    }

    public EntityMSkill FindBySkillId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindBySkillId(int key, out EntityMSkill result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
