using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMConfigTable : TableBase<EntityMConfig>
{
    private readonly Func<EntityMConfig, string> primaryIndexSelector;

    public EntityMConfigTable(EntityMConfig[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ConfigKey;
    }

    public EntityMConfig FindByConfigKey(string key) => FindUniqueCore(data, primaryIndexSelector, Comparer<string>.Default, key);
}
