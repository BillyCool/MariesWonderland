using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusAbilityTable : TableBase<EntityMQuestBonusAbility>
{
    private readonly Func<EntityMQuestBonusAbility, int> primaryIndexSelector;

    public EntityMQuestBonusAbilityTable(EntityMQuestBonusAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestBonusEffectId;
    }

    public EntityMQuestBonusAbility FindByQuestBonusEffectId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByQuestBonusEffectId(int key, out EntityMQuestBonusAbility result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
