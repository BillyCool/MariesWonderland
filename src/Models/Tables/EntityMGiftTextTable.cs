using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMGiftTextTable : TableBase<EntityMGiftText>
{
    private readonly Func<EntityMGiftText, (int, LanguageType)> primaryIndexSelector;

    public EntityMGiftTextTable(EntityMGiftText[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.GiftTextId, element.LanguageType);
    }

    public EntityMGiftText FindByGiftTextIdAndLanguageType(ValueTuple<int, LanguageType> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(int, LanguageType)>.Default, key);
}
