using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMComboCalculationSetting))]
[PrimaryKey(nameof(ComboCountLowerLimit))]
public class EntityMComboCalculationSetting
{
    [Key(0)] public int ComboCountLowerLimit { get; set; }

    [Key(1)] public int DamageCoefficientPermil { get; set; }

    [Key(2)] public int UiEffectIndex { get; set; }
}
