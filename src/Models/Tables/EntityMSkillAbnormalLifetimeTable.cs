using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalLifetimeTable : TableBase<EntityMSkillAbnormalLifetime>
{
    private readonly Func<EntityMSkillAbnormalLifetime, int> primaryIndexSelector;

    public EntityMSkillAbnormalLifetimeTable(EntityMSkillAbnormalLifetime[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalLifetimeId;
    }

    public EntityMSkillAbnormalLifetime FindBySkillAbnormalLifetimeId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
