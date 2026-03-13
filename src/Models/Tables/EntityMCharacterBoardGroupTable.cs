using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardGroupTable : TableBase<EntityMCharacterBoardGroup>
{
    private readonly Func<EntityMCharacterBoardGroup, int> primaryIndexSelector;

    public EntityMCharacterBoardGroupTable(EntityMCharacterBoardGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterBoardGroupId;
    }

    public EntityMCharacterBoardGroup FindByCharacterBoardGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
