using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeCollectionBonus
{
    public int CollectionBonusId { get; set; }

    public int CollectionBonusTextId { get; set; }

    public int CollectionBonusGroupId { get; set; }

    public int CollectionBonusQuestAssignmentGroupId { get; set; }

    public int CollectionBonusEffectId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int GroupingId { get; set; }
}
