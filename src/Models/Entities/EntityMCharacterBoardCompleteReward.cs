using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCharacterBoardCompleteReward))]
public class EntityMCharacterBoardCompleteReward
{
    [Key(0)] public int CharacterBoardCompleteRewardId { get; set; }

    [Key(1)] public int CharacterBoardCompleteRewardGroupId { get; set; }

    [Key(2)] public int CharacterBoardCompleteRewardConditionGroupId { get; set; }
}
