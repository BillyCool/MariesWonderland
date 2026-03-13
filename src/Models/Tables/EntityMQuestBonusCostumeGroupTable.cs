using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusCostumeGroupTable : TableBase<EntityMQuestBonusCostumeGroup>
{
    private readonly Func<EntityMQuestBonusCostumeGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestBonusCostumeGroupTable(EntityMQuestBonusCostumeGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestBonusCostumeGroupId, element.CostumeId);
    }
}
