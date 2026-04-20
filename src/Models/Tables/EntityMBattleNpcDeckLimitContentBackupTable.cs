using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckLimitContentBackupTable : TableBase<EntityMBattleNpcDeckLimitContentBackup>
{
    private readonly Func<EntityMBattleNpcDeckLimitContentBackup, (long, int, int)> primaryIndexSelector;

    public EntityMBattleNpcDeckLimitContentBackupTable(EntityMBattleNpcDeckLimitContentBackup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.EventQuestChapterId, element.EventQuestSequenceSortOrder);
    }
}
