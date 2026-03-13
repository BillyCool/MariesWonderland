using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsStatusMain
{
    public int PartsStatusMainId { get; set; }

    public StatusKindType StatusKindType { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int StatusChangeInitialValue { get; set; }

    public int StatusNumericalFunctionId { get; set; }
}
