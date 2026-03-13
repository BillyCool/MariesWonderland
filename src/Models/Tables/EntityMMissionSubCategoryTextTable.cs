using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionSubCategoryTextTable : TableBase<EntityMMissionSubCategoryText>
{
    private readonly Func<EntityMMissionSubCategoryText, int> primaryIndexSelector;

    public EntityMMissionSubCategoryTextTable(EntityMMissionSubCategoryText[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionSubCategoryId;
    }

    public EntityMMissionSubCategoryText FindByMissionSubCategoryId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
