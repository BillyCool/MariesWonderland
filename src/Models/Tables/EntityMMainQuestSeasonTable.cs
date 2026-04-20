using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMainQuestSeasonTable : TableBase<EntityMMainQuestSeason>
{
    private readonly Func<EntityMMainQuestSeason, int> primaryIndexSelector;

    public EntityMMainQuestSeasonTable(EntityMMainQuestSeason[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MainQuestSeasonId;
    }

    public EntityMMainQuestSeason FindByMainQuestSeasonId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
