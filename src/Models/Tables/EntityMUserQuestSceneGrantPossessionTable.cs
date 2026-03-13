using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMUserQuestSceneGrantPossessionTable : TableBase<EntityMUserQuestSceneGrantPossession>
{
    private readonly Func<EntityMUserQuestSceneGrantPossession, (int, PossessionType, int)> primaryIndexSelector;

    public EntityMUserQuestSceneGrantPossessionTable(EntityMUserQuestSceneGrantPossession[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestSceneId, element.PossessionType, element.PossessionId);
    }

    public RangeView<EntityMUserQuestSceneGrantPossession> FindRangeByQuestSceneIdAndPossessionTypeAndPossessionId(ValueTuple<int, PossessionType, int> min, ValueTuple<int, PossessionType, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, PossessionType, int)>.Default, min, max, ascendant);
}
