using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterLevelBonusAbilityGroup))]
[PrimaryKey(nameof(CharacterLevelBonusAbilityGroupId), nameof(ActivationCharacterLevel))]
public class EntityMCharacterLevelBonusAbilityGroup
{
    [Key(0)] public int CharacterLevelBonusAbilityGroupId { get; set; }

    [Key(1)] public int ActivationCharacterLevel { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int AbilityLevel { get; set; }
}
