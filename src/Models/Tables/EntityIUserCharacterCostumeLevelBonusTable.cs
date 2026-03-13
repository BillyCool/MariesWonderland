using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCharacterCostumeLevelBonusTable : TableBase<EntityIUserCharacterCostumeLevelBonus>
{
    private readonly Func<EntityIUserCharacterCostumeLevelBonus, (long, int, StatusCalculationType)> primaryIndexSelector;
    private readonly Func<EntityIUserCharacterCostumeLevelBonus, int> secondaryIndexSelector;

    public EntityIUserCharacterCostumeLevelBonusTable(EntityIUserCharacterCostumeLevelBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.CharacterId, element.StatusCalculationType);
        secondaryIndexSelector = element => element.CharacterId;
    }

    public bool TryFindByUserIdAndCharacterIdAndStatusCalculationType(ValueTuple<long, int, StatusCalculationType> key, out EntityIUserCharacterCostumeLevelBonus result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int, StatusCalculationType)>.Default, key, out result);

    public RangeView<EntityIUserCharacterCostumeLevelBonus> FindByCharacterId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
