using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>Evaluates master data numerical functions (costs, caps, sell prices).</summary>
public static class NumericalFunctionHelper
{
    /// <summary>Evaluates the function at <paramref name="value"/>; 0 when it doesn't exist or its type is unsupported.</summary>
    public static int Evaluate(DarkMasterMemoryDatabase masterDb, int functionId, int value)
    {
        EntityMNumericalFunction? func = masterDb.EntityMNumericalFunction.FirstOrDefault(f => f.NumericalFunctionId == functionId);
        if (func == null)
        {
            return 0;
        }

        int[] p = [.. masterDb.EntityMNumericalFunctionParameterGroup
            .Where(pg => pg.NumericalFunctionParameterGroupId == func.NumericalFunctionParameterGroupId)
            .OrderBy(pg => pg.ParameterIndex)
            .Select(pg => pg.ParameterValue)];

        return func.NumericalFunctionType switch
        {
            NumericalFunctionType.LINEAR when p.Length >= 2 => p[1] + p[0] * value,
            NumericalFunctionType.MONOMIAL when p.Length >= 2 => EvaluateMonomial(p, value),
            NumericalFunctionType.LINEAR_PERMIL when p.Length >= 2 => p[0] * value / 1000 + p[1],
            NumericalFunctionType.POLYNOMIAL_THIRD when p.Length >= 4 => p[3] + (p[2] + (p[1] + p[0] * value) * value) * value,
            NumericalFunctionType.POLYNOMIAL_THIRD_PERMIL when p.Length >= 4 =>
                p[0] * value * value * value / 1000 +
                p[1] * value * value / 1000 +
                p[2] * value / 1000 +
                p[3],
            _ => 0
        };
    }

    private static int EvaluateMonomial(int[] p, int value)
    {
        int v = value - 1;
        int result = v;
        int counter = p[1];
        while (counter > 1)
        {
            result *= v;
            counter--;
        }

        return p[0] * result;
    }
}
