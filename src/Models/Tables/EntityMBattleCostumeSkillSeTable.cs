using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleCostumeSkillSeTable : TableBase<EntityMBattleCostumeSkillSe>
{
    private readonly Func<EntityMBattleCostumeSkillSe, int> primaryIndexSelector;

    public EntityMBattleCostumeSkillSeTable(EntityMBattleCostumeSkillSe[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeId;
    }

    public bool TryFindByCostumeId(int key, out EntityMBattleCostumeSkillSe result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
