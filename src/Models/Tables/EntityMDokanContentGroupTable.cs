using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMDokanContentGroupTable : TableBase<EntityMDokanContentGroup>
{
    private readonly Func<EntityMDokanContentGroup, (int, int)> primaryIndexSelector;

    public EntityMDokanContentGroupTable(EntityMDokanContentGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.DokanContentGroupId, element.ContentIndex);
    }
}
