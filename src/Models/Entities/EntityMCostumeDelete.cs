using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeDelete
{
    public int CostumeId { get; set; }

    public int DeleteConditionClearQuestId { get; set; }

    public int CostumeAlternativeGroupId { get; set; }

    public TutorialType DeleteCostumeTutorialType { get; set; }

    public int MaterialReturnGiftGrantRouteType { get; set; }
}
