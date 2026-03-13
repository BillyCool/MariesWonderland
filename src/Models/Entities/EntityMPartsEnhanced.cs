using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsEnhanced
{
    public int PartsEnhancedId { get; set; }

    public int PartsId { get; set; }

    public int PartsStatusMainId { get; set; }

    public int Level { get; set; }

    public bool IsRandomSubStatusCount { get; set; }

    public int SubStatusCount { get; set; }
}
