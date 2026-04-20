using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMQuestBonusCharacterGroupTable : TableBase<EntityMQuestBonusCharacterGroup>
{
    private readonly Func<EntityMQuestBonusCharacterGroup, (int, int)> primaryIndexSelector;

    public EntityMQuestBonusCharacterGroupTable(EntityMQuestBonusCharacterGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestBonusCharacterGroupId, element.CharacterId);
    }

    public RangeView<EntityMQuestBonusCharacterGroup> FindRangeByQuestBonusCharacterGroupIdAndCharacterId(ValueTuple<int, int> min, ValueTuple<int, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, min, max, ascendant);
}
