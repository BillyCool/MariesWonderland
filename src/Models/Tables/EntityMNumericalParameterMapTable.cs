using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMNumericalParameterMapTable : TableBase<EntityMNumericalParameterMap>
{
    private readonly Func<EntityMNumericalParameterMap, (int, int)> primaryIndexSelector;

    public EntityMNumericalParameterMapTable(EntityMNumericalParameterMap[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.NumericalParameterMapId, element.ParameterKey);
    }

    public EntityMNumericalParameterMap FindByNumericalParameterMapIdAndParameterKey(ValueTuple<int, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key);
}
