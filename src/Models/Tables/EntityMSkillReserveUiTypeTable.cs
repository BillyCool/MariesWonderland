using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillReserveUiTypeTable : TableBase<EntityMSkillReserveUiType>
{
    private readonly Func<EntityMSkillReserveUiType, int> primaryIndexSelector;

    public EntityMSkillReserveUiTypeTable(EntityMSkillReserveUiType[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillDetailId;
    }

    public bool TryFindBySkillDetailId(int key, out EntityMSkillReserveUiType result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
