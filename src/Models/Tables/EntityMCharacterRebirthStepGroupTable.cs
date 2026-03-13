using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterRebirthStepGroupTable : TableBase<EntityMCharacterRebirthStepGroup>
{
    private readonly Func<EntityMCharacterRebirthStepGroup, (int, int)> primaryIndexSelector;

    public EntityMCharacterRebirthStepGroupTable(EntityMCharacterRebirthStepGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.CharacterRebirthStepGroupId, element.BeforeRebirthCount);
    }

    public EntityMCharacterRebirthStepGroup FindByCharacterRebirthStepGroupIdAndBeforeRebirthCount(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
