using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickExtraQuestTable : TableBase<EntityMGimmickExtraQuest>
{
    private readonly Func<EntityMGimmickExtraQuest, (int, int)> primaryIndexSelector;

    public EntityMGimmickExtraQuestTable(EntityMGimmickExtraQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.GimmickId, element.GimmickOrnamentIndex);
    }

    public EntityMGimmickExtraQuest FindByGimmickIdAndGimmickOrnamentIndex(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public bool TryFindByGimmickIdAndGimmickOrnamentIndex(ValueTuple<int, int> key, out EntityMGimmickExtraQuest result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
