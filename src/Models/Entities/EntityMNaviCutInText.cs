using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMNaviCutInText
{
    public int NaviCutInTextId { get; set; }

    public LanguageType LanguageType { get; set; }

    public string Text { get; set; }
}
