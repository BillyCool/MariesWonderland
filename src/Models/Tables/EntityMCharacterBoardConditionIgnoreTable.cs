using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardConditionIgnoreTable : TableBase<EntityMCharacterBoardConditionIgnore>
{
    private readonly Func<EntityMCharacterBoardConditionIgnore, (int, int)> primaryIndexSelector;

    public EntityMCharacterBoardConditionIgnoreTable(EntityMCharacterBoardConditionIgnore[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterBoardConditionIgnoreId, element.IgnoreIndex);
    }
}
