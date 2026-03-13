using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMEventQuestGuerrillaFreeOpenTable : TableBase<EntityMEventQuestGuerrillaFreeOpen>
{
    private readonly Func<EntityMEventQuestGuerrillaFreeOpen, int> primaryIndexSelector;

    public EntityMEventQuestGuerrillaFreeOpenTable(EntityMEventQuestGuerrillaFreeOpen[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.EventQuestGuerrillaFreeOpenId;
    }
}
