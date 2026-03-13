using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMExtraQuestGroupTable : TableBase<EntityMExtraQuestGroup>
{
    private readonly Func<EntityMExtraQuestGroup, (int, int)> primaryIndexSelector;

    public EntityMExtraQuestGroupTable(EntityMExtraQuestGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestId, element.ExtraQuestIndex);
    }
}
