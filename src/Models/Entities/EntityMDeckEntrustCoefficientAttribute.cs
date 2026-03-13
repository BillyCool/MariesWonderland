using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMDeckEntrustCoefficientAttribute
{
    public AttributeType EntrustAttributeType { get; set; }

    public AttributeType AttributeType { get; set; }

    public int CoefficientPermil { get; set; }
}
