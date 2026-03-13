using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCollectionBonusQuestAssignment
{
    public int CollectionBonusQuestAssignmentId { get; set; }

    public QuestAssignmentType QuestAssignmentType { get; set; }

    public int MainQuestChapterId { get; set; }

    public int EventQuestChapterId { get; set; }

    public int QuestId { get; set; }

    public int SortOrder { get; set; }
}
