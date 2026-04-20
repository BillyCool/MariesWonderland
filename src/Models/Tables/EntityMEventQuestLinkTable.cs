using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestLinkTable : TableBase<EntityMEventQuestLink>
{
    private readonly Func<EntityMEventQuestLink, int> primaryIndexSelector;

    public EntityMEventQuestLinkTable(EntityMEventQuestLink[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EventQuestLinkId;
    }

    public EntityMEventQuestLink FindByEventQuestLinkId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
