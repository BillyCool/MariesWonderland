using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCompleteMissionGroup
{
    public int MissionId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int SortOrder { get; set; }
}
