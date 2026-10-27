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

        return Evaluate(func.NumericalFunctionType, p, value);
    }

    /// <summary>Evaluates a function of <paramref name="type"/> with parameters <paramref name="p"/> (in index order) at <paramref name="value"/>.</summary>
    public static int Evaluate(NumericalFunctionType type, int[] p, int value)
    {
        // Products are 64-bit like the client's; the result is narrowed at the end.
        long v = value;
        return (int)(type switch
        {
            NumericalFunctionType.LINEAR when p.Length >= 2 => p[1] + p[0] * v,
            NumericalFunctionType.MONOMIAL when p.Length >= 2 => EvaluateMonomial(p, v),
            NumericalFunctionType.LINEAR_PERMIL when p.Length >= 2 => p[0] * v / 1000 + p[1],
            NumericalFunctionType.POLYNOMIAL_THIRD when p.Length >= 4 => p[3] + (p[2] + (p[1] + p[0] * v) * v) * v,
            NumericalFunctionType.POLYNOMIAL_THIRD_PERMIL when p.Length >= 4 =>
                p[0] * v * v * v / 1000 +
                p[1] * v * v / 1000 +
                p[2] * v / 1000 +
                p[3],
            _ => 0
        });
    }

    private static long EvaluateMonomial(int[] p, long value)
    {
        long v = value - 1;
        long result = v;
        int counter = p[1];
        while (counter > 1)
        {
            result *= v;
            counter--;
        }

        return p[0] * result;
    }
}
