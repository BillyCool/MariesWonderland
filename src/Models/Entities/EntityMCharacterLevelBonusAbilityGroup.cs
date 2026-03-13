using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCharacterLevelBonusAbilityGroup
{
    public int CharacterLevelBonusAbilityGroupId { get; set; }

    public int ActivationCharacterLevel { get; set; }

    public int AbilityId { get; set; }

    public int AbilityLevel { get; set; }
}
