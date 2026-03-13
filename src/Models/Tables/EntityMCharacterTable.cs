using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCharacterTable : TableBase<EntityMCharacter>
{
    private readonly Func<EntityMCharacter, int> primaryIndexSelector;
    private readonly Func<EntityMCharacter, int> secondaryIndexSelector;

    public EntityMCharacterTable(EntityMCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CharacterId;
        secondaryIndexSelector = element => element.EndWeaponId;
    }

    public EntityMCharacter FindByCharacterId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public RangeView<EntityMCharacter> FindByEndWeaponId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
