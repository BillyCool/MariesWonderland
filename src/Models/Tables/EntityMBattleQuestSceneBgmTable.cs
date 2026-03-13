using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleQuestSceneBgmTable : TableBase<EntityMBattleQuestSceneBgm>
{
    private readonly Func<EntityMBattleQuestSceneBgm, (int, int)> primaryIndexSelector;

    public EntityMBattleQuestSceneBgmTable(EntityMBattleQuestSceneBgm[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.QuestSceneId, element.StartWaveNumber);
    }
}
