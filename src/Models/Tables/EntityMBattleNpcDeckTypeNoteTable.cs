using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcDeckTypeNoteTable : TableBase<EntityMBattleNpcDeckTypeNote>
{
    private readonly Func<EntityMBattleNpcDeckTypeNote, (long, DeckType)> primaryIndexSelector;

    public EntityMBattleNpcDeckTypeNoteTable(EntityMBattleNpcDeckTypeNote[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.DeckType);
    }
}
