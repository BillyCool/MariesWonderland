using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardConditionDetailTable : TableBase<EntityMCharacterBoardConditionDetail>
{
    private readonly Func<EntityMCharacterBoardConditionDetail, (int, int)> primaryIndexSelector;

    public EntityMCharacterBoardConditionDetailTable(EntityMCharacterBoardConditionDetail[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardConditionDetailId, element.DetailIndex);
    }

    public EntityMCharacterBoardConditionDetail FindByCharacterBoardConditionDetailIdAndDetailIndex(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);

    public EntityMCharacterBoardConditionDetail FindClosestByCharacterBoardConditionDetailIdAndDetailIndex(ValueTuple<int, int> key, bool selectLower = true) =>
        FindUniqueClosestCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, selectLower);
}
