using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterBoardCompleteReward))]
[PrimaryKey(nameof(BattleNpcId), nameof(CharacterBoardCompleteRewardId))]
public class EntityMBattleNpcCharacterBoardCompleteReward
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterBoardCompleteRewardId { get; set; }
}
