using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMMainQuestSequenceGroupTable : TableBase<EntityMMainQuestSequenceGroup>
{
    private readonly Func<EntityMMainQuestSequenceGroup, (int, DifficultyType)> primaryIndexSelector;

    public EntityMMainQuestSequenceGroupTable(EntityMMainQuestSequenceGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.MainQuestSequenceGroupId, element.DifficultyType);
    }

    public EntityMMainQuestSequenceGroup FindByMainQuestSequenceGroupIdAndDifficultyType(ValueTuple<int, DifficultyType> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, DifficultyType)>.Default, key);

    public bool TryFindByMainQuestSequenceGroupIdAndDifficultyType(ValueTuple<int, DifficultyType> key, out EntityMMainQuestSequenceGroup result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, DifficultyType)>.Default, key, out result);
}
