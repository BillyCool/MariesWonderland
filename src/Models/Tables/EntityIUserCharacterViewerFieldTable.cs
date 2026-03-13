using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCharacterViewerFieldTable : TableBase<EntityIUserCharacterViewerField>
{
    private readonly Func<EntityIUserCharacterViewerField, (long, int)> primaryIndexSelector;

    public EntityIUserCharacterViewerFieldTable(EntityIUserCharacterViewerField[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.CharacterViewerFieldId);
    }
}
