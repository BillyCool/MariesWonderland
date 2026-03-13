using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillDamageMultiplyDetailHitIndexTable : TableBase<EntityMSkillDamageMultiplyDetailHitIndex>
{
    private readonly Func<EntityMSkillDamageMultiplyDetailHitIndex, int> primaryIndexSelector;

    public EntityMSkillDamageMultiplyDetailHitIndexTable(EntityMSkillDamageMultiplyDetailHitIndex[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillDamageMultiplyDetailId;
    }

    public EntityMSkillDamageMultiplyDetailHitIndex FindBySkillDamageMultiplyDetailId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
