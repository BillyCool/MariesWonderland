using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckBackupTable : TableBase<EntityMBattleNpcDeckBackup>
{
    private readonly Func<EntityMBattleNpcDeckBackup, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcDeckBackupTable(EntityMBattleNpcDeckBackup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcDeckBackupUuid);
    }
}
