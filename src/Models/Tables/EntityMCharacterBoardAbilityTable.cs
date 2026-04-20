using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterBoardAbilityTable : TableBase<EntityMCharacterBoardAbility>
{
    private readonly Func<EntityMCharacterBoardAbility, int> primaryIndexSelector;

    public EntityMCharacterBoardAbilityTable(EntityMCharacterBoardAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterBoardAbilityId;
    }

    public bool TryFindByCharacterBoardAbilityId(int key, out EntityMCharacterBoardAbility result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
