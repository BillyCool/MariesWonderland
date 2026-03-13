using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleAttributeDamageCoefficientGroup
{
    public int AttributeDamageCoefficientGroupId { get; set; }

    public AttributeType SkillExecutorAttributeType { get; set; }

    public AttributeType SkillTargetAttributeType { get; set; }

    public AttributeCompatibilityType AttributeCompatibilityType { get; set; }

    public int DamageCoefficientPermil { get; set; }
}
