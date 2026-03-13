using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMGimmickSequenceRewardGroupTable : TableBase<EntityMGimmickSequenceRewardGroup>
{
    private readonly Func<EntityMGimmickSequenceRewardGroup, (int, int)> primaryIndexSelector;

    public EntityMGimmickSequenceRewardGroupTable(EntityMGimmickSequenceRewardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.GimmickSequenceRewardGroupId, element.GroupIndex);
    }

    public bool TryFindByGimmickSequenceRewardGroupIdAndGroupIndex(ValueTuple<int, int> key, out EntityMGimmickSequenceRewardGroup result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
