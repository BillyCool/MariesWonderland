using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardConditionGroupTable : TableBase<EntityMCharacterBoardConditionGroup>
{
    private readonly Func<EntityMCharacterBoardConditionGroup, int> primaryIndexSelector;

    public EntityMCharacterBoardConditionGroupTable(EntityMCharacterBoardConditionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterBoardConditionGroupId;
    }

    public EntityMCharacterBoardConditionGroup FindByCharacterBoardConditionGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
