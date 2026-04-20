using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcPartsGroupNoteTable : TableBase<EntityMBattleNpcPartsGroupNote>
{
    private readonly Func<EntityMBattleNpcPartsGroupNote, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcPartsGroupNoteTable(EntityMBattleNpcPartsGroupNote[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.PartsGroupId);
    }
}
