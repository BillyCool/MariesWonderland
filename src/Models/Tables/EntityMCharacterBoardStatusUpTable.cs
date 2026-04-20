using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardStatusUpTable : TableBase<EntityMCharacterBoardStatusUp>
{
    private readonly Func<EntityMCharacterBoardStatusUp, int> primaryIndexSelector;

    public EntityMCharacterBoardStatusUpTable(EntityMCharacterBoardStatusUp[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterBoardStatusUpId;
    }

    public bool TryFindByCharacterBoardStatusUpId(int key, out EntityMCharacterBoardStatusUp result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
