using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckLimitContentBackupRestoredTable : TableBase<EntityMBattleNpcDeckLimitContentBackupRestored>
{
    private readonly Func<EntityMBattleNpcDeckLimitContentBackupRestored, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcDeckLimitContentBackupRestoredTable(EntityMBattleNpcDeckLimitContentBackupRestored[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.EventQuestChapterId);
    }
}
