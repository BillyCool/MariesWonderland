using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcDeckLimitContentRestricted
{
    public long BattleNpcId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int QuestId { get; set; }

    public string DeckRestrictedUuid { get; set; }

    public PossessionType PossessionType { get; set; }

    public string TargetUuid { get; set; }
}
