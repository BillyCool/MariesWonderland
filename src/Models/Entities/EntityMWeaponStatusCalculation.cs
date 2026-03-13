using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponStatusCalculation
{
    public int WeaponStatusCalculationId { get; set; }

    public int HpNumericalFunctionId { get; set; }

    public int AttackNumericalFunctionId { get; set; }

    public int VitalityNumericalFunctionId { get; set; }
}
