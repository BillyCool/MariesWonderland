using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMGiftText
{
    public int GiftTextId { get; set; }

    public LanguageType LanguageType { get; set; }

    public string Text { get; set; }
}
