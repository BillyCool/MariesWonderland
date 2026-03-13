using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMDokanText
{
    public int DokanTextId { get; set; }

    public LanguageType LanguageType { get; set; }

    public string Text { get; set; }
}
