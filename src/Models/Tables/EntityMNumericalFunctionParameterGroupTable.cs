using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMNumericalFunctionParameterGroupTable : TableBase<EntityMNumericalFunctionParameterGroup>
{
    private readonly Func<EntityMNumericalFunctionParameterGroup, (int, int)> primaryIndexSelector;
    private readonly Func<EntityMNumericalFunctionParameterGroup, int> secondaryIndexSelector;

    public EntityMNumericalFunctionParameterGroupTable(EntityMNumericalFunctionParameterGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.NumericalFunctionParameterGroupId, element.ParameterIndex);
        secondaryIndexSelector = element => element.NumericalFunctionParameterGroupId;
    }

    public RangeView<EntityMNumericalFunctionParameterGroup> FindByNumericalFunctionParameterGroupId(int key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
