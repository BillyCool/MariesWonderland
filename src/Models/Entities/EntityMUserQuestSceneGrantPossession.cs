using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMUserQuestSceneGrantPossession
{
    public int QuestSceneId { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }

    public int Count { get; set; }

    public bool IsGift { get; set; }

    public bool IsDebug { get; set; }
}
