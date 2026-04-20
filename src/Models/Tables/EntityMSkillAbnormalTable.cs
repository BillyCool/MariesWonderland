using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalTable : TableBase<EntityMSkillAbnormal>
{
    private readonly Func<EntityMSkillAbnormal, int> primaryIndexSelector;

    public EntityMSkillAbnormalTable(EntityMSkillAbnormal[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalId;
    }

    public EntityMSkillAbnormal FindBySkillAbnormalId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
