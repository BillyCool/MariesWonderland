using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBigHuntBoss
{
    public int BigHuntBossId { get; set; }

    public int BigHuntBossGradeGroupId { get; set; }

    public int NameBigHuntBossTextId { get; set; }

    public int BigHuntBossAssetId { get; set; }

    public AttributeType AttributeType { get; set; }
}
