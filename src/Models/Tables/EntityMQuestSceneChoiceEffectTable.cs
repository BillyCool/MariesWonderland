using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestSceneChoiceEffectTable : TableBase<EntityMQuestSceneChoiceEffect>
{
    private readonly Func<EntityMQuestSceneChoiceEffect, int> primaryIndexSelector;
    private readonly Func<EntityMQuestSceneChoiceEffect, (int, int)> secondaryIndexSelector;

    public EntityMQuestSceneChoiceEffectTable(EntityMQuestSceneChoiceEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestSceneChoiceEffectId;
        secondaryIndexSelector = element => (element.QuestSceneChoiceCostumeEffectGroupId, element.QuestSceneChoiceWeaponEffectGroupId);
    }

    public RangeView<EntityMQuestSceneChoiceEffect> FindByQuestSceneChoiceCostumeEffectGroupId(int key)
    {
        var result = data
            .Where(x => x.QuestSceneChoiceCostumeEffectGroupId == key)
            .ToArray();

        return new RangeView<EntityMQuestSceneChoiceEffect>(result, 0, result.Length - 1, true);
    }
}
