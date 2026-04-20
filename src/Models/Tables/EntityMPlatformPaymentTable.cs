using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMPlatformPaymentTable : TableBase<EntityMPlatformPayment>
{
    private readonly Func<EntityMPlatformPayment, (int, PlatformType)> primaryIndexSelector;

    public EntityMPlatformPaymentTable(EntityMPlatformPayment[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PlatformPaymentId, element.PlatformType);
    }

    public EntityMPlatformPayment FindByPlatformPaymentIdAndPlatformType(ValueTuple<int, PlatformType> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, PlatformType)>.Default, key);
}
