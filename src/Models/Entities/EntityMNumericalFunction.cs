using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMNumericalFunction
{
    public int NumericalFunctionId { get; set; }

    public NumericalFunctionType NumericalFunctionType { get; set; }

    public int NumericalFunctionParameterGroupId { get; set; }
}
