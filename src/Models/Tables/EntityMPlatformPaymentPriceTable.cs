using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMPlatformPaymentPriceTable : TableBase<EntityMPlatformPaymentPrice>
{
    private readonly Func<EntityMPlatformPaymentPrice, (int, PlatformType)> primaryIndexSelector;

    public EntityMPlatformPaymentPriceTable(EntityMPlatformPaymentPrice[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PlatformPaymentId, element.PlatformType);
    }

    public EntityMPlatformPaymentPrice FindByPlatformPaymentIdAndPlatformType(ValueTuple<int, PlatformType> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, PlatformType)>.Default, key);
}
