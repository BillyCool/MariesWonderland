using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMExtraQuestGroupInMainQuestChapter
{
    public int MainQuestChapterId { get; set; }

    public int ExtraQuestIndex { get; set; }

    public int ExtraQuestId { get; set; }
}
