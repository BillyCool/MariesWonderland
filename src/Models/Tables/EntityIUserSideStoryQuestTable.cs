using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserSideStoryQuestTable : TableBase<EntityIUserSideStoryQuest>
{
    private readonly Func<EntityIUserSideStoryQuest, (long, int)> primaryIndexSelector;

    public EntityIUserSideStoryQuestTable(EntityIUserSideStoryQuest[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.SideStoryQuestId);
    }
}
