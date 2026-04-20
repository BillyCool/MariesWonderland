using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardTable : TableBase<EntityMCharacterBoard>
{
    private readonly Func<EntityMCharacterBoard, int> primaryIndexSelector;

    public EntityMCharacterBoardTable(EntityMCharacterBoard[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterBoardId;
    }

    public EntityMCharacterBoard FindByCharacterBoardId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
