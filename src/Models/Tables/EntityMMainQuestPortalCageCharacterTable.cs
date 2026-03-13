using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMainQuestPortalCageCharacterTable : TableBase<EntityMMainQuestPortalCageCharacter>
{
    private readonly Func<EntityMMainQuestPortalCageCharacter, int> primaryIndexSelector;

    public EntityMMainQuestPortalCageCharacterTable(EntityMMainQuestPortalCageCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestSceneId;
    }
}
