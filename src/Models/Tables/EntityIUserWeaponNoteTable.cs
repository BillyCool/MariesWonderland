using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserWeaponNoteTable : TableBase<EntityIUserWeaponNote>
{
    private readonly Func<EntityIUserWeaponNote, (long, int)> primaryIndexSelector;

    public EntityIUserWeaponNoteTable(EntityIUserWeaponNote[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.WeaponId);
    }

    public bool TryFindByUserIdAndWeaponId(ValueTuple<long, int> key, out EntityIUserWeaponNote result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
