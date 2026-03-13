using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusAllyCharacterTable : TableBase<EntityMQuestBonusAllyCharacter>
{
    private readonly Func<EntityMQuestBonusAllyCharacter, int> primaryIndexSelector;

    public EntityMQuestBonusAllyCharacterTable(EntityMQuestBonusAllyCharacter[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.QuestBonusAllyCharacterId;
    }

    public bool TryFindByQuestBonusAllyCharacterId(int key, out EntityMQuestBonusAllyCharacter result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
