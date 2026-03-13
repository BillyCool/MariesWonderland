using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeStatusCalculation
{
    public int CostumeStatusCalculationId { get; set; }

    public int HpNumericalFunctionId { get; set; }

    public int AttackNumericalFunctionId { get; set; }

    public int VitalityNumericalFunctionId { get; set; }

    public int AgilityNumericalFunctionId { get; set; }

    public int CriticalRatioPermilNumericalFunctionId { get; set; }

    public int CriticalAttackRatioPermilNumericalFunctionId { get; set; }

    public int EvasionRatioPermilNumericalFunctionId { get; set; }
}
