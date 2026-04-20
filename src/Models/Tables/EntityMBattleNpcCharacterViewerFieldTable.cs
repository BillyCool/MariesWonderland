using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterViewerFieldTable : TableBase<EntityMBattleNpcCharacterViewerField>
{
    private readonly Func<EntityMBattleNpcCharacterViewerField, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcCharacterViewerFieldTable(EntityMBattleNpcCharacterViewerField[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterViewerFieldId);
    }
}
