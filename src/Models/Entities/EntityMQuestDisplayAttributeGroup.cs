using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestDisplayAttributeGroup
{
    public int QuestDisplayAttributeGroupId { get; set; }

    public int SortOrder { get; set; }

    public QuestDisplayAttributeType QuestDisplayAttributeType { get; set; }

    public QuestDisplayAttributeIconSizeType QuestDisplayAttributeIconSizeType { get; set; }
}
