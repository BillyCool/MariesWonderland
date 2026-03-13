using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCharacterBoardAbilityTable : TableBase<EntityIUserCharacterBoardAbility>
{
    private readonly Func<EntityIUserCharacterBoardAbility, (long, int, int)> primaryIndexSelector;

    public EntityIUserCharacterBoardAbilityTable(EntityIUserCharacterBoardAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.CharacterId, element.AbilityId);
    }

    public bool TryFindByUserIdAndCharacterIdAndAbilityId(ValueTuple<long, int, int> key, out EntityIUserCharacterBoardAbility result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, int)>.Default, key, out result);
}
