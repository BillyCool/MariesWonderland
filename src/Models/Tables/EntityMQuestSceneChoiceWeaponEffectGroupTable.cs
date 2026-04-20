using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestSceneChoiceWeaponEffectGroupTable : TableBase<EntityMQuestSceneChoiceWeaponEffectGroup>
{
    private readonly Func<EntityMQuestSceneChoiceWeaponEffectGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMQuestSceneChoiceWeaponEffectGroup, int> secondaryIndexSelector;

    public EntityMQuestSceneChoiceWeaponEffectGroupTable(EntityMQuestSceneChoiceWeaponEffectGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestSceneChoiceWeaponEffectGroupId, element.SortOrder);
        secondaryIndexSelector = element => element.WeaponId;
    }

    public RangeView<EntityMQuestSceneChoiceWeaponEffectGroup> FindByWeaponId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
