using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalDamageMultiplyDetailHitIndexTable : TableBase<EntityMSkillAbnormalDamageMultiplyDetailHitIndex>
{
    private readonly Func<EntityMSkillAbnormalDamageMultiplyDetailHitIndex, int> primaryIndexSelector;

    public EntityMSkillAbnormalDamageMultiplyDetailHitIndexTable(EntityMSkillAbnormalDamageMultiplyDetailHitIndex[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.DamageMultiplyAbnormalDetailId;
    }

    public EntityMSkillAbnormalDamageMultiplyDetailHitIndex FindByDamageMultiplyAbnormalDetailId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
